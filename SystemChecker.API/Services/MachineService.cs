using Microsoft.EntityFrameworkCore;
using SystemChecker.API.Data;
using SystemChecker.API.Models;

namespace SystemChecker.API.Services;

public class MachineService : IMachineService
{
    private readonly ApiDbContext _context;
    private readonly IApiKeyService _apiKeyService;
    private readonly ILogger<MachineService> _logger;

    public MachineService(
        ApiDbContext context,
        IApiKeyService apiKeyService,
        ILogger<MachineService> logger)
    {
        _context = context;
        _apiKeyService = apiKeyService;
        _logger = logger;
    }

    public async Task<(string ApiKey, string MachineName)> RegisterMachineAsync(string machineName, string? description)
    {
        _logger.LogInformation("Registering new machine: {MachineName}", machineName);
        
        var apiKey = await _apiKeyService.GenerateApiKeyAsync(machineName, description);
        
        return (apiKey.Key, apiKey.Machine.Name);
    }

    public async Task<Machine?> GetMachineByIdAsync(int machineId)
    {
        return await _context.Machines
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == machineId);
    }

    public async Task<IEnumerable<Machine>> GetAllMachinesAsync()
    {
        return await _context.Machines
            .Include(m => m.ApiKey)
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .ToListAsync();
    }
} 