using Microsoft.Extensions.Options;
using System.ServiceProcess;
using SystemChecker.Core.Models;
using SystemChecker.Infrastructure.Settings;

namespace SystemChecker.Infrastructure.Services;

public interface IServiceChecker
{
    Task<ServiceStatus[]> CheckServicesAsync(List<string> servicesToMonitor);
}

public class ServiceChecker : IServiceChecker
{
    private readonly IOptions<ServiceSettings> _settings;

    public ServiceChecker(
        IOptions<ServiceSettings> settings)
    {
        _settings = settings;
    }

    public async Task<ServiceStatus[]> CheckServicesAsync(List<string> servicesToMonitor)
    {
        var allServices = ServiceController.GetServices();
        var results = new List<ServiceStatus>();

        foreach (var serviceName in servicesToMonitor)
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