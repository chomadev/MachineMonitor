using SystemChecker.Core.Interfaces;
using SystemChecker.Core.Models;

namespace SystemChecker.Infrastructure.Services;

public class SystemCheckService : ISystemCheckService
{
    private readonly IServiceChecker _serviceChecker;
    private readonly INetworkChecker _networkChecker;
    private readonly IDiskChecker _diskChecker;
    private readonly IResourceChecker _resourceChecker;
    private readonly IConfigurationService _configService;
    private readonly ITcpPortChecker _portChecker;
    private readonly IFolderMonitor _folderMonitor;

    public SystemCheckService(
        IServiceChecker serviceChecker,
        INetworkChecker networkChecker,
        IDiskChecker diskChecker,
        IResourceChecker resourceChecker,
        IConfigurationService configService,
        ITcpPortChecker portChecker,
        IFolderMonitor folderMonitor)
    {
        _serviceChecker = serviceChecker;
        _networkChecker = networkChecker;
        _diskChecker = diskChecker;
        _resourceChecker = resourceChecker;
        _configService = configService;
        _portChecker = portChecker;
        _folderMonitor = folderMonitor;
    }

    public async Task<SystemCheck> PerformSystemCheckAsync()
    {
        var services = await _serviceChecker.CheckServicesAsync(_configService.GetMonitoredServices());
        var network = await _networkChecker.CheckNetworkAsync();
        var disk = await _diskChecker.CheckDisksAsync();
        var resourceCpu = await _resourceChecker.CheckCpuAsync();
        var resourceMemory = await _resourceChecker.CheckMemoryAsync();

        return new SystemCheck
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.Now,
            Services = services,
            Network = network,
            Disks = disk,
            Cpu = resourceCpu,
            Memory = resourceMemory,
            Ports = await _portChecker.CheckPortsAsync(),
            FolderChanges = _folderMonitor.GetChanges()
        };
    }

    public async Task<bool> PushCheckResultAsync(SystemCheck check)
    {
        // Implementação do envio para API será adicionada posteriormente
        return true;
    }
}