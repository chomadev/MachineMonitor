namespace SystemChecker.Core.Interfaces;

public interface IConfigurationService
{
    string GetCurrentSchedule();
    List<string> GetMonitoredServices();
    string GetMonitoredPorts();
    
    Task UpdateScheduleAsync(string cronExpression);
    Task UpdateMonitoredServicesAsync(List<string> services);
    Task UpdateMonitoredPortsAsync(IEnumerable<int> ports);
    
    event EventHandler ConfigurationChanged;
}
