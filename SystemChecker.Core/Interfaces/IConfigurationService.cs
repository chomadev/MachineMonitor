namespace SystemChecker.Core.Interfaces;

public interface IConfigurationService
{
    string GetCurrentSchedule();
    List<string> GetMonitoredServices();
    Task UpdateConfiguration(string cronExpression, List<string> services);
}
