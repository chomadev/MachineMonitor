using Microsoft.EntityFrameworkCore;
using SystemChecker.API.Data;
using SystemChecker.API.Models;

namespace SystemChecker.API.Services;

public interface ISystemCheckHistoryService
{
    Task<SystemCheckHistory> SaveCheckAsync(string apiKey, SystemCheckData checkData);
    Task<SystemCheckHistory?> GetLatestCheckAsync(string apiKey);
    Task<IEnumerable<SystemCheckHistory>> GetCheckHistoryAsync(string apiKey, DateTime? from = null, DateTime? to = null);
}

public class SystemCheckHistoryService : ISystemCheckHistoryService
{
    private readonly ApiDbContext _context;
    private readonly IApiKeyService _apiKeyService;
    private readonly ILogger<SystemCheckHistoryService> _logger;

    public SystemCheckHistoryService(
        ApiDbContext context,
        IApiKeyService apiKeyService,
        ILogger<SystemCheckHistoryService> logger)
    {
        _context = context;
        _apiKeyService = apiKeyService;
        _logger = logger;
    }

    public async Task<SystemCheckHistory> SaveCheckAsync(string apiKey, SystemCheckData checkData)
    {
        var key = await _apiKeyService.GetApiKeyAsync(apiKey);
        if (key == null)
            throw new UnauthorizedAccessException("API Key inválida");

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
                IpAddress = checkData.Network.IpAddress
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
            }).ToList()
        };

        _context.SystemCheckHistory.Add(history);
        await _context.SaveChangesAsync();

        await _apiKeyService.UpdateLastUsedAsync(apiKey);

        return history;
    }

    public async Task<SystemCheckHistory?> GetLatestCheckAsync(string apiKey)
    {
        var key = await _apiKeyService.GetApiKeyAsync(apiKey);
        if (key == null)
            throw new UnauthorizedAccessException("API Key inválida");

        return await _context.SystemCheckHistory
            .Include(h => h.Services)
            .Include(h => h.Network)
            .Include(h => h.Disks)
            .Include(h => h.Cpu)
            .Include(h => h.Memory)
            .Include(h => h.Ports)
            .Where(h => h.MachineId == key.MachineId)
            .OrderByDescending(h => h.Timestamp)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<SystemCheckHistory>> GetCheckHistoryAsync(
        string apiKey,
        DateTime? from = null,
        DateTime? to = null)
    {
        var key = await _apiKeyService.GetApiKeyAsync(apiKey);
        if (key == null)
            throw new UnauthorizedAccessException("API Key inválida");

        var query = _context.SystemCheckHistory
            .Include(h => h.Services)
            .Include(h => h.Network)
            .Include(h => h.Disks)
            .Include(h => h.Cpu)
            .Include(h => h.Memory)
            .Include(h => h.Ports)
            .Where(h => h.MachineId == key.MachineId);

        if (from.HasValue)
            query = query.Where(h => h.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(h => h.Timestamp <= to.Value);

        return await query
            .OrderByDescending(h => h.Timestamp)
            .ToListAsync();
    }
} 