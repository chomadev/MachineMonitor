using Microsoft.EntityFrameworkCore;
using SystemChecker.API.Data;
using SystemChecker.API.Models;

namespace SystemChecker.API.Services;

public class ApiKeyService : IApiKeyService
{
    private readonly ApiDbContext _context;
    private readonly ILogger<ApiKeyService> _logger;

    public ApiKeyService(ApiDbContext context, ILogger<ApiKeyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> IsValidApiKeyAsync(string apiKey)
    {
        var key = await _context.ApiKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Key == apiKey && k.IsActive);
            
        return key != null;
    }

    public async Task<ApiKey> GenerateApiKeyAsync(string machineName, string? description = null)
    {
        var machine = new Machine
        {
            Name = machineName,
            Description = description
        };

        var apiKey = new ApiKey
        {
            Key = GenerateUniqueKey(),
            CreatedAt = DateTime.UtcNow,
            Machine = machine
        };

        _context.ApiKeys.Add(apiKey);
        await _context.SaveChangesAsync();

        return apiKey;
    }

    public async Task<ApiKey?> GetApiKeyAsync(string key)
    {
        return await _context.ApiKeys
            .Include(k => k.Machine)
            .ThenInclude(k => k.MachineConfiguration)
            .FirstOrDefaultAsync(k => k.Key == key);
    }

    private string GenerateUniqueKey()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "_")
            .Replace("+", "-")
            .Replace("=", "");
    }
} 