using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using SystemChecker.Core.Interfaces;
using SystemChecker.Core.Models;

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
    private readonly IConfiguration _configuration;
    private MachineConfiguration _currentConfig;

    public SystemCheckService(
        ILogger<SystemCheckService> logger,
        IServiceChecker serviceChecker,
        INetworkChecker networkChecker,
        IDiskChecker diskChecker,
        IResourceChecker resourceChecker,
        IConfigurationService configService,
        ITcpPortService tcpPortService,
        IFolderMonitor folderMonitor,
        HttpClient httpClient,
        IConfiguration configuration)
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
        _configuration = configuration;
        _currentConfig = new MachineConfiguration();

        _configService.ConfigurationChanged += async (_, _) =>
        {
            _currentConfig = await _configService.LoadConfigurationAsync();
            _logger.LogInformation("System check configuration updated");
        };

        // Carrega configuração inicial
        Task.Run(async () =>
        {
            try
            {
                _currentConfig = await _configService.LoadConfigurationAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading initial configuration");
            }
        });
    }

    public async Task<SystemCheck> PerformSystemCheckAsync()
    {
        try
        {
            _logger.LogInformation("Starting system check...");

            var systemCheck = new SystemCheck
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };

            // Check services
            systemCheck.Services = await _serviceChecker.CheckServicesAsync(_currentConfig.ServicesToMonitor);

            // Check network
            systemCheck.Network = await _networkChecker.CheckNetworkAsync();

            // Check disks
            systemCheck.Disks = await _diskChecker.CheckDisksAsync();

            // Check CPU and Memory
            systemCheck.Cpu = await _resourceChecker.CheckCpuAsync();
            systemCheck.Memory = await _resourceChecker.CheckMemoryAsync();

            // Check TCP ports
            systemCheck.Ports = (await _tcpPortService.CheckPortsAsync(_currentConfig.TcpPorts)).ToArray();

            // Check folders
            systemCheck.Folders = await _folderMonitor.CheckFoldersAsync(_currentConfig.MonitoredFolders);
            systemCheck.FolderChanges = _folderMonitor.GetChanges();

            _logger.LogInformation("System check completed successfully");
            return systemCheck;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing system check");
            throw;
        }
    }

    public async Task<bool> PushCheckResultAsync(SystemCheck check)
    {
        try
        {
            _logger.LogInformation("Starting to send check results to API...");

            // Adicionar apiKey como parâmetro de consulta
            var apiSettings = _configuration.GetSection("ApiSettings");
            var baseUrl = apiSettings["BaseUrl"];
            var apiKey = apiSettings["MachineKey"];
            var url = $"{baseUrl}/api/systemcheck?apiKey={apiKey}";

            // Enviar o objeto SystemCheck
            var response = await _httpClient.PostAsJsonAsync(url, check);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Results sent successfully to API");
                return true;
            }

            // Ler o conteúdo da resposta para melhor diagnóstico
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to send results to API. Status: {StatusCode}, Content: {Content}",
                response.StatusCode, content);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending results to API");
            return false;
        }
    }
}