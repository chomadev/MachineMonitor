using Microsoft.EntityFrameworkCore;
using SystemChecker.API.Data;
using SystemChecker.API.Models;
using System.Text.Json;

namespace SystemChecker.API.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly ApiDbContext _context;
    private readonly IApiKeyService _apiKeyService;
    private readonly ILogger<ConfigurationService> _logger;

    public ConfigurationService(
        ApiDbContext context,
        IApiKeyService apiKeyService,
        ILogger<ConfigurationService> logger)
    {
        _context = context;
        _apiKeyService = apiKeyService;
        _logger = logger;
    }

    public async Task<MachineConfiguration?> GetConfigurationAsync(string apiKey)
    {
        var key = await _apiKeyService.GetApiKeyAsync(apiKey);
        if (key == null)
            throw new UnauthorizedAccessException("Invalid API key");

        var config = await _context.MachineConfigurations
            .Include(c => c.MonitoredFolders)
            .FirstOrDefaultAsync(c => c.MachineId == key.MachineId);

        return config;
    }

    public async Task<MachineConfiguration> SaveConfigurationAsync(string apiKey, MachineConfiguration configuration)
    {
        var key = await _apiKeyService.GetApiKeyAsync(apiKey);
        if (key == null)
            throw new UnauthorizedAccessException("Invalid API key");

        var existingConfig = await _context.MachineConfigurations
            .Include(c => c.MonitoredFolders)
            .FirstOrDefaultAsync(c => c.MachineId == key.MachineId);

        if (existingConfig == null)
        {
            existingConfig = new MachineConfiguration
            {
                MachineId = key.MachineId
            };
            _context.MachineConfigurations.Add(existingConfig);
        }

        // Atualiza as propriedades
        existingConfig.CheckSchedule = configuration.CheckSchedule;
        existingConfig.ServicesToMonitor = configuration.ServicesToMonitor;
        existingConfig.IpAddressesToMonitor = configuration.IpAddressesToMonitor;
        existingConfig.TcpPorts = configuration.TcpPorts;
        existingConfig.LastUpdated = DateTime.UtcNow;

        // Atualiza as pastas monitoradas
        existingConfig.MonitoredFolders.Clear();
        foreach (var folder in configuration.MonitoredFolders)
        {
            existingConfig.MonitoredFolders.Add(new FolderMonitorConfig
            {
                Path = folder.Path,
                ShouldBeEmpty = folder.ShouldBeEmpty,
                MonitorLastModified = folder.MonitorLastModified,
                CheckZeroByteFiles = folder.CheckZeroByteFiles
            });
        }

        await _context.SaveChangesAsync();
        return existingConfig;
    }
}