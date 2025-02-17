using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemChecker.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SystemChecker.Infrastructure.Settings;

namespace SystemChecker.WPF.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly string _configPath;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonOptions;
    public event EventHandler ConfigurationChanged;
    private readonly ILogger<ConfigurationService> _logger;

    public ConfigurationService(IConfiguration configuration, ILogger<ConfigurationService> logger)
    {
        _configuration = configuration;
        _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        _logger = logger;
    }

    public string GetCurrentSchedule()
    {
        return _configuration.GetSection("SchedulerSettings:CheckSchedule").Value ?? "*/1 * * * *";
    }

    public List<string> GetMonitoredServices()
    {
        var services = _configuration.GetSection("ServiceSettings:ServicesToMonitor")
            .Get<string[]>() ?? Array.Empty<string>();
        return services.ToList();
    }

    public string GetMonitoredPorts()
    {
        var ports = _configuration.GetSection("TcpPortSettings:Ports")
            .Get<int[]>() ?? Array.Empty<int>();
        return string.Join(",", ports);
    }

    public async Task UpdateMonitoredPortsAsync(IEnumerable<int> ports)
    {
        var config = await LoadConfigurationFile();
        var oldPorts = GetMonitoredPorts();
        UpdateTcpPortSettings(config, string.Join(",", ports));
        await SaveConfigurationFile(config);
        
        _logger.LogInformation(
            "TCP Ports configuration changed from [{OldPorts}] to [{NewPorts}]", 
            oldPorts, 
            string.Join(",", ports));
        
        ConfigurationChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task UpdateScheduleAsync(string cronExpression)
    {
        var config = await LoadConfigurationFile();
        var oldSchedule = GetCurrentSchedule();
        UpdateSchedulerSettings(config, cronExpression);
        await SaveConfigurationFile(config);
        
        _logger.LogInformation(
            "Schedule changed from '{OldSchedule}' to '{NewSchedule}'", 
            oldSchedule, 
            cronExpression);
        
        ConfigurationChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task UpdateMonitoredServicesAsync(List<string> services)
    {
        var config = await LoadConfigurationFile();
        var oldServices = GetMonitoredServices();
        UpdateServiceSettings(config, services);
        await SaveConfigurationFile(config);
        
        _logger.LogInformation(
            "Monitored services changed from [{OldServices}] to [{NewServices}]", 
            string.Join(",", oldServices), 
            string.Join(",", services));
        
        ConfigurationChanged?.Invoke(this, EventArgs.Empty);
    }

    public FolderMonitorSettings GetFolderMonitorSettings()
    {
        var settings = _configuration.GetSection("FolderMonitorSettings").Get<FolderMonitorSettings>();
        return settings ?? new FolderMonitorSettings();
    }

    public async Task UpdateFolderMonitorSettingsAsync(FolderMonitorSettings settings)
    {
        var config = await LoadConfigurationFile();
        var oldSettings = GetFolderMonitorSettings();
        
        var json = JsonSerializer.Serialize(settings);
        config["FolderMonitorSettings"] = JsonDocument.Parse(json).RootElement;
        
        await SaveConfigurationFile(config);
        
        _logger.LogInformation(
            "Folder monitor settings updated. Folders: [{Folders}], Extensions: [{Extensions}]",
            string.Join(",", settings.FoldersToMonitor),
            string.Join(",", settings.FileExtensionsToMonitor));
        
        ConfigurationChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task<Dictionary<string, JsonElement>> LoadConfigurationFile()
    {
        var jsonString = await File.ReadAllTextAsync(_configPath);
        return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString) 
            ?? new Dictionary<string, JsonElement>();
    }

    private async Task SaveConfigurationFile(Dictionary<string, JsonElement> config)
    {
        var jsonString = JsonSerializer.Serialize(config, _jsonOptions);
        await File.WriteAllTextAsync(_configPath, jsonString);
    }

    private void UpdateSchedulerSettings(Dictionary<string, JsonElement> config, string cronExpression)
    {
        var settings = new { CheckSchedule = cronExpression };
        var json = JsonSerializer.Serialize(settings);
        config["SchedulerSettings"] = JsonDocument.Parse(json).RootElement;
    }

    private void UpdateServiceSettings(Dictionary<string, JsonElement> config, List<string> services)
    {
        var settings = new { ServicesToMonitor = services.ToArray() };
        var json = JsonSerializer.Serialize(settings);
        config["ServiceSettings"] = JsonDocument.Parse(json).RootElement;
    }

    private void UpdateTcpPortSettings(Dictionary<string, JsonElement> config, string tcpPorts)
    {
        var ports = tcpPorts.Split(',')
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => int.Parse(p.Trim()))
            .ToArray();

        var settings = new { Ports = ports };
        var json = JsonSerializer.Serialize(settings);
        config["TcpPortSettings"] = JsonDocument.Parse(json).RootElement;
    }
} 