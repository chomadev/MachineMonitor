namespace SystemChecker.Core.Interfaces;
using SystemChecker.Core.Models;

public interface IConfigurationService
{
    Task<MachineConfiguration> LoadConfigurationAsync();
    Task SaveConfigurationAsync(MachineConfiguration config);
    
    event EventHandler ConfigurationChanged;
}
