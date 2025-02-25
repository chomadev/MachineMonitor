using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SystemChecker.API.Data;
using SystemChecker.API.Models;
using SystemChecker.API.Models.Dto;
using System.Text.Json;

namespace SystemChecker.API.Services;

public interface ISystemCheckHistoryService
{
    Task<SystemCheckHistory> SaveCheckAsync(string apiKey, SystemCheckDto checkData);
    Task<SystemCheckHistory?> GetLatestCheckAsync(string apiKey);
    Task<IEnumerable<SystemCheckHistory>> GetCheckHistoryAsync(string apiKey, DateTime? from = null, DateTime? to = null);
}

public class SystemCheckHistoryService : ISystemCheckHistoryService
{
    private readonly ApiDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<SystemCheckHistoryService> _logger;

    public SystemCheckHistoryService(
        ApiDbContext context,
        IMapper mapper,
        ILogger<SystemCheckHistoryService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SystemCheckHistory> SaveCheckAsync(string apiKey, SystemCheckDto checkData)
    {
        var key = await _context.ApiKeys
            .Include(k => k.Machine)
            .FirstOrDefaultAsync(k => k.Key == apiKey);

        if (key == null)
            throw new UnauthorizedAccessException("API Key is invalid");

        var history = new SystemCheckHistory
        {
            Timestamp = checkData.Timestamp,
            MachineId = key.MachineId,
            Services = checkData.Services.Select(s => new ServiceStatus
            {
                Name = s.Name,
                DisplayName = s.DisplayName,
                Status = s.Status,
                IsRunning = s.IsRunning
            }).ToList(),
            Network = checkData.Network == null ? null : new NetworkStatus
            {
                IsConnected = checkData.Network.IsConnected,
                HasInternetAccess = checkData.Network.HasInternetAccess,
                IpAddress = checkData.Network.IpAddress,
                ActiveInterfaces = checkData.Network.ActiveInterfaces != null ? 
                    JsonSerializer.Serialize(checkData.Network.ActiveInterfaces) : "[]",
                MonitoredAddresses = checkData.Network.MonitoredAddresses.Select(a => new MonitoredAddress
                {
                    Address = a.Address,
                    IsReachable = a.IsReachable,
                    ResponseTime = a.ResponseTime
                }).ToList()
            },
            Disks = checkData.Disks.Select(d => new DiskStatus
            {
                Name = d.Name,
                TotalSpace = d.TotalSpace,
                FreeSpace = d.FreeSpace,
                UsagePercentage = d.UsagePercentage
            }).ToList(),
            Cpu = checkData.Cpu == null ? null : new CpuStatus
            {
                UsagePercentage = checkData.Cpu.UsagePercentage
            },
            Memory = checkData.Memory == null ? null : new MemoryStatus
            {
                TotalPhysicalMemory = checkData.Memory.TotalPhysicalMemory,
                AvailablePhysicalMemory = checkData.Memory.AvailablePhysicalMemory,
                UsagePercentage = checkData.Memory.UsagePercentage
            },
            Ports = checkData.Ports.Select(p => new TcpPortStatus
            {
                Port = p.Port,
                IsOpen = p.IsOpen,
                Service = p.Service
            }).ToList(),
            Folders = checkData.Folders.Select(f => new FolderStatus
            {
                Path = f.Path,
                Exists = f.Exists,
                IsEmpty = f.IsEmpty,
                LastModified = f.LastModified,
                HasZeroByteFiles = f.HasZeroByteFiles,
                IsValid = f.IsValid,
                ErrorMessage = f.ErrorMessage,
                ZeroByteFiles = f.ZeroByteFiles != null ? 
                    JsonSerializer.Serialize(f.ZeroByteFiles) : "[]"
            }).ToList(),
            FolderChanges = checkData.FolderChanges.Select(f => new FolderChange
            {
                Path = f.Path,
                LastChanged = f.LastChanged,
                LastChangeType = f.LastChangeType
            }).ToList()
        };

        _context.SystemCheckHistory.Add(history);
        await _context.SaveChangesAsync();

        return history;
    }

    public async Task<SystemCheckHistory?> GetLatestCheckAsync(string apiKey)
    {
        var key = await _context.ApiKeys
            .FirstOrDefaultAsync(k => k.Key == apiKey);

        if (key == null)
            throw new UnauthorizedAccessException("API Key is invalid");

        var check = await _context.SystemCheckHistory
            .Include(x => x.Services)
            .Include(x => x.Network)
                .ThenInclude(n => n!.MonitoredAddresses)
            .Include(x => x.Disks)
            .Include(x => x.Cpu)
            .Include(x => x.Memory)
            .Include(x => x.Ports)
            .Include(x => x.Folders)
            .Include(x => x.FolderChanges)
            .Include(x => x.Machine)
            .Where(x => x.MachineId == key.MachineId)
            .OrderByDescending(x => x.Timestamp)
            .FirstOrDefaultAsync();

        if (check == null)
            return null;

        return check;
    }

    public async Task<IEnumerable<SystemCheckHistory>> GetCheckHistoryAsync(
        string apiKey,
        DateTime? from = null,
        DateTime? to = null)
    {
        var key = await _context.ApiKeys
            .FirstOrDefaultAsync(k => k.Key == apiKey);

        if (key == null)
            throw new UnauthorizedAccessException("API Key is invalid");

        var query = _context.SystemCheckHistory
            .Include(h => h.Services)
            .Include(h => h.Network)
            .ThenInclude(n => n!.MonitoredAddresses)
            .Include(h => h.Disks)
            .Include(h => h.Cpu)
            .Include(h => h.Memory)
            .Include(h => h.Ports)
            .Include(h => h.Folders)
            .Include(h => h.FolderChanges)
            .Where(h => h.MachineId == key.MachineId);

        if (from.HasValue)
            query = query.Where(h => h.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(h => h.Timestamp <= to.Value);

        var results = await query
            .OrderByDescending(h => h.Timestamp)
            .ToListAsync();

        return results;
    }
} 