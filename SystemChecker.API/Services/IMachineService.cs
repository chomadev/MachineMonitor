using SystemChecker.API.Models;

namespace SystemChecker.API.Services;

public interface IMachineService
{
    Task<(string ApiKey, string MachineName)> RegisterMachineAsync(string machineName, string? description);
    Task<Machine?> GetMachineByIdAsync(int machineId);
    Task<IEnumerable<Machine>> GetAllMachinesAsync();
} 