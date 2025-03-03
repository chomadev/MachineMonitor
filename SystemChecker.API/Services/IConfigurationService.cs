using SystemChecker.API.Models;

namespace SystemChecker.API.Services;

public interface IConfigurationService
{
    Task<MachineConfiguration?> GetConfigurationAsync(string apiKey);
    Task<MachineConfiguration> SaveConfigurationAsync(string apiKey, MachineConfiguration configuration);
} 