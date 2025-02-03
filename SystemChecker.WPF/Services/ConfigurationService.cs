using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemChecker.Core.Interfaces;

namespace SystemChecker.WPF.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly string _configPath;
    private readonly IConfiguration _configuration;

    public ConfigurationService(IConfiguration configuration)
    {
        _configuration = configuration;
        _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
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

    public async Task UpdateConfiguration(string cronExpression, List<string> services)
    {
        // Lê o arquivo existente
        var jsonString = await File.ReadAllTextAsync(_configPath);
        var configObject = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString);

        // Atualiza o agendamento
        var schedulerSettings = new
        {
            CheckSchedule = cronExpression
        };
        var schedulerJson = JsonSerializer.Serialize(schedulerSettings);

        // Atualiza os serviços
        var serviceSettings = new
        {
            ServicesToMonitor = services.ToArray()
        };
        var serviceJson = JsonSerializer.Serialize(serviceSettings);

        // Atualiza o objeto de configuração
        configObject["SchedulerSettings"] = JsonDocument.Parse(schedulerJson).RootElement;
        configObject["ServiceSettings"] = JsonDocument.Parse(serviceJson).RootElement;

        // Salva o arquivo atualizado
        var options = new JsonSerializerOptions { WriteIndented = true };
        var updatedJson = JsonSerializer.Serialize(configObject, options);
        await File.WriteAllTextAsync(_configPath, updatedJson);
    }

    public string GetCronExpression()
    {
        return _configuration.GetValue<string>("SchedulerSettings:CheckSchedule") ?? "*/5 * * * *"; // padrão: a cada 5 minutos
    }

    public IEnumerable<int> GetMonitoredPorts()
    {
        var ports = _configuration.GetSection("TcpPortSettings:Ports")
            .Get<int[]>() ?? Array.Empty<int>();
        return ports;
    }

    public async Task UpdateMonitoredPortsAsync(IEnumerable<int> ports)
    {
        var jsonString = await File.ReadAllTextAsync(_configPath);
        var configObject = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString);

        var portSettings = new { Ports = ports.ToArray() };
        var portJson = JsonSerializer.Serialize(portSettings);

        configObject["TcpPortSettings"] = JsonDocument.Parse(portJson).RootElement;

        var options = new JsonSerializerOptions { WriteIndented = true };
        var updatedJson = JsonSerializer.Serialize(configObject, options);
        await File.WriteAllTextAsync(_configPath, updatedJson);
    }
} 