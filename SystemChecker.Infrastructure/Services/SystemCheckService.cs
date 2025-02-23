using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SystemChecker.Core.Interfaces;
using SystemChecker.Core.Models;
using SystemChecker.Infrastructure.Settings;

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
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl;

    public SystemCheckService(
        ILogger<SystemCheckService> logger,
        IServiceChecker serviceChecker,
        INetworkChecker networkChecker,
        IDiskChecker diskChecker,
        IResourceChecker resourceChecker,
        IConfigurationService configService,
        ITcpPortService tcpPortService,
        IFolderMonitor folderMonitor,
        IOptions<ApiSettings> apiSettings,
        HttpClient httpClient)
    {
        _logger = logger;
        _serviceChecker = serviceChecker;
        _networkChecker = networkChecker;
        _diskChecker = diskChecker;
        _resourceChecker = resourceChecker;
        _configService = configService;
        _tcpPortService = tcpPortService;
        _folderMonitor = folderMonitor;
        _httpClient = httpClient;
        
        // Remove redirecionamento HTTPS
        _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        _apiUrl = $"{apiSettings.Value.BaseUrl}/api/systemcheck?apiKey={apiSettings.Value.MachineKey}";

        // Subscribe to configuration changes
        _configService.ConfigurationChanged += (_, _) =>
        {
            _logger.LogInformation("Configuration changed - next check will use updated settings");
        };
    }

    public async Task<SystemCheck> PerformSystemCheckAsync()
    {
        var systemCheck = new SystemCheck
        {
            Timestamp = DateTime.Now
        };

        try
        {
            // Services Check - uses current configuration
            _logger.LogInformation("Starting services check...");
            systemCheck.Services = await _serviceChecker.CheckServicesAsync(_configService.GetMonitoredServices());
            _logger.LogInformation("Services check completed");

            // Resources Check
            _logger.LogInformation("Starting resources check...");
            systemCheck.Cpu = await _resourceChecker.CheckCpuAsync();
            systemCheck.Memory = await _resourceChecker.CheckMemoryAsync();
            _logger.LogInformation("Resources check completed");

            // Network Check
            _logger.LogInformation("Starting network check...");
            systemCheck.Network = await _networkChecker.CheckNetworkAsync();
            _logger.LogInformation("Network check completed: Connected={Connected}, HasInternet={HasInternet}, MonitoredIPs={IPs}", 
                systemCheck.Network.IsConnected, 
                systemCheck.Network.HasInternetAccess,
                string.Join(",", _configService.GetMonitoredIpAddresses()));

            // TCP Ports Check - uses current configuration via TcpPortService
            _logger.LogInformation("Starting TCP ports check...");
            var configuredPorts = _tcpPortService.GetConfiguredPorts();
            systemCheck.Ports = (await _tcpPortService.CheckPortsAsync(configuredPorts)).ToArray();
            _logger.LogInformation("TCP ports check completed. Checked {count} ports", systemCheck.Ports.Length);

            // Disks Check
            _logger.LogInformation("Starting disks check...");
            systemCheck.Disks = (await _diskChecker.CheckDisksAsync()).ToArray();
            _logger.LogInformation("Disks check completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing system check");
            throw;
        }

        return systemCheck;
    }

    public async Task<bool> PushCheckResultAsync(SystemCheck check)
    {
        try
        {
            _logger.LogInformation("Starting to send check results to API...");
            
            var response = await _httpClient.PostAsJsonAsync(_apiUrl, check);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Results sent successfully to API");
                return true;
            }
            
            _logger.LogError("Failed to send results to API. Status: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending results to API");
            return false;
        }
    }
}