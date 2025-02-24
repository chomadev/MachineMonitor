namespace SystemChecker.Core.Interfaces;
using SystemChecker.Core.Models;

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
    
    List<FolderMonitorConfig> GetMonitoredFolders();
    Task UpdateMonitoredFoldersAsync(List<FolderMonitorConfig> folders);
    
    event EventHandler ConfigurationChanged;
}
