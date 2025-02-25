using Microsoft.Extensions.Logging;
using System.ServiceProcess;
using SystemChecker.Core.Interfaces;
using SystemChecker.Core.Models;

namespace SystemChecker.Infrastructure.Services;

public interface IServiceChecker
{
    Task<ServiceStatus[]> CheckServicesAsync(IEnumerable<string> services);
}

public class ServiceChecker : IServiceChecker
{
    private readonly ILogger<ServiceChecker> _logger;
    private readonly IConfigurationService _configService;
    private MachineConfiguration _currentConfig;

    public ServiceChecker(
        ILogger<ServiceChecker> logger,
        IConfigurationService configService)
    {
        _logger = logger;
        _configService = configService;
        _currentConfig = new MachineConfiguration();

        _configService.ConfigurationChanged += async (_, _) =>
        {
            _currentConfig = await _configService.LoadConfigurationAsync();
            _logger.LogInformation("Services configuration updated");
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

    public async Task<ServiceStatus[]> CheckServicesAsync(IEnumerable<string> services)
    {
        var servicesToCheck = services ?? _currentConfig.ServicesToMonitor;
        var allServices = ServiceController.GetServices();
        var results = new List<ServiceStatus>();

        foreach (var serviceName in servicesToCheck)
        {
            var serviceController = allServices.FirstOrDefault(s =>
                s.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));

            if (serviceController != null)
            {
                try
                {
                    results.Add(new ServiceStatus
                    {
                        Name = serviceController.ServiceName,
                        IsRunning = serviceController.Status == ServiceControllerStatus.Running,
                        Status = serviceController.Status.ToString()
                    });
                }
                catch (Exception)
                {
                    results.Add(new ServiceStatus
                    {
                        Name = serviceController.ServiceName,
                        IsRunning = false,
                        Status = "Error"
                    });
                }
            }
            else
            {
                results.Add(new ServiceStatus
                {
                    Name = serviceName,
                    IsRunning = false,
                    Status = "Not Found"
                });
            }
        }

        return results.ToArray();
    }
}