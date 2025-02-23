namespace SystemChecker.Core.Interfaces;

public interface IConfigurationService
{
    string GetCurrentSchedule();
    List<string> GetMonitoredServices();
    string GetMonitoredPorts();
    List<string> GetMonitoredIpAddresses();
    
    Task UpdateScheduleAsync(string cronExpression);
    Task UpdateMonitoredServicesAsync(List<string> services);
    Task UpdateMonitoredPortsAsync(IEnumerable<int> ports);
    Task UpdateMonitoredIpAddressesAsync(List<string> ipAddresses);
    
    event EventHandler ConfigurationChanged;
}
