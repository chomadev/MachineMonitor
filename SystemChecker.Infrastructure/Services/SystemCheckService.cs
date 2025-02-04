using SystemChecker.Core.Interfaces;
using SystemChecker.Core.Models;
using Microsoft.Extensions.Logging;

namespace SystemChecker.Infrastructure.Services;

public class SystemCheckService : ISystemCheckService
{
    private readonly ILogger<SystemCheckService> _logger;
    private readonly IServiceChecker _serviceChecker;
    private readonly INetworkChecker _networkChecker;
    private readonly IDiskChecker _diskChecker;
    private readonly IResourceChecker _resourceChecker;
    private readonly IConfigurationService _configService;
    private readonly ITcpPortService _tcpPortService;
    private readonly IFolderMonitor _folderMonitor;

    public SystemCheckService(
        ILogger<SystemCheckService> logger,
        IServiceChecker serviceChecker,
        INetworkChecker networkChecker,
        IDiskChecker diskChecker,
        IResourceChecker resourceChecker,
        IConfigurationService configService,
        ITcpPortService tcpPortService,
        IFolderMonitor folderMonitor)
    {
        _logger = logger;
        _serviceChecker = serviceChecker;
        _networkChecker = networkChecker;
        _diskChecker = diskChecker;
        _resourceChecker = resourceChecker;
        _configService = configService;
        _tcpPortService = tcpPortService;
        _folderMonitor = folderMonitor;
    }

    public async Task<SystemCheck> PerformSystemCheckAsync()
    {
        var systemCheck = new SystemCheck
        {
            Timestamp = DateTime.Now
        };

        // Verifica serviços
        _logger.LogInformation("Starting services check...");
        systemCheck.Services = await _serviceChecker.CheckServicesAsync(_configService.GetMonitoredServices());
        _logger.LogInformation("Services check completed");

        // Verifica recursos (CPU/Memória)
        _logger.LogInformation("Iniciando verificação de recursos (CPU/Memória)...");
        systemCheck.Cpu = await _resourceChecker.CheckCpuAsync();
        systemCheck.Memory = await _resourceChecker.CheckMemoryAsync();
        _logger.LogInformation("Verificação de recursos concluída");

        // Verifica rede
        _logger.LogInformation("Iniciando verificação de rede...");
        systemCheck.Network = await _networkChecker.CheckNetworkAsync();
        _logger.LogInformation("Verificação de rede concluída");

        // Verifica portas TCP
        _logger.LogInformation("Iniciando verificação de portas TCP...");
        var ports = _tcpPortService.GetConfiguredPorts();
        systemCheck.Ports = (await _tcpPortService.CheckPortsAsync(ports)).ToArray();
        _logger.LogInformation("Verificação de portas TCP concluída");

        return systemCheck;
    }

    public async Task<bool> PushCheckResultAsync(SystemCheck check)
    {
        _logger.LogInformation("Iniciando envio dos resultados da verificação...");
        // Implementação do envio para API será adicionada posteriormente
        await Task.CompletedTask;
        _logger.LogInformation("Envio dos resultados concluído");
        return true;
    }
}