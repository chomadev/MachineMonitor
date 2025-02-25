using SystemChecker.API.Models;

public interface IApiKeyService
{
    Task<bool> IsValidApiKeyAsync(string apiKey);
    Task<ApiKey> GenerateApiKeyAsync(string machineName, string? description = null);
    Task<ApiKey?> GetApiKeyAsync(string key);
}