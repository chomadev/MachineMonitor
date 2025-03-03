using System;
using System.Collections.Generic;
using System.Text.Json;
using SystemChecker.Core.Models;

namespace SystemChecker.WPF.Models
{
    public class ApiMachineConfigurationDto
    {
        public int Id { get; set; }
        public int MachineId { get; set; }
        public string CheckSchedule { get; set; } = "*/5 * * * *";
        public string ServicesToMonitor { get; set; } = "[]"; // JSON serialized
        public string IpAddressesToMonitor { get; set; } = "[]"; // JSON serialized
        public string TcpPorts { get; set; } = "[]"; // JSON serialized
        public List<ApiFolderMonitorConfigDto> MonitoredFolders { get; set; } = new();
        public DateTime LastUpdated { get; set; }

        // Método para converter para o modelo do Core
        public MachineConfiguration ToMachineConfiguration()
        {
            return new MachineConfiguration
            {
                CheckSchedule = this.CheckSchedule,
                ServicesToMonitor = JsonSerializer.Deserialize<List<string>>(this.ServicesToMonitor) ?? new(),
                IpAddressesToMonitor = JsonSerializer.Deserialize<List<string>>(this.IpAddressesToMonitor) ?? new(),
                TcpPorts = JsonSerializer.Deserialize<List<int>>(this.TcpPorts) ?? new(),
                MonitoredFolders = this.MonitoredFolders.Select(f => f.ToFolderMonitorConfig()).ToList(),
                LastUpdated = this.LastUpdated
            };
        }

        // Método para converter do modelo do Core para o DTO
        public static ApiMachineConfigurationDto FromMachineConfiguration(MachineConfiguration config)
        {
            return new ApiMachineConfigurationDto
            {
                CheckSchedule = config.CheckSchedule,
                ServicesToMonitor = JsonSerializer.Serialize(config.ServicesToMonitor),
                IpAddressesToMonitor = JsonSerializer.Serialize(config.IpAddressesToMonitor),
                TcpPorts = JsonSerializer.Serialize(config.TcpPorts),
                MonitoredFolders = config.MonitoredFolders.Select(f => 
                    ApiFolderMonitorConfigDto.FromFolderMonitorConfig(f)).ToList(),
                LastUpdated = config.LastUpdated
            };
        }
    }

    public class ApiFolderMonitorConfigDto
    {
        public int Id { get; set; }
        public int MachineConfigurationId { get; set; }
        public string Path { get; set; } = string.Empty;
        public bool ShouldBeEmpty { get; set; }
        public bool MonitorLastModified { get; set; }
        public bool CheckZeroByteFiles { get; set; }

        public FolderMonitorConfig ToFolderMonitorConfig()
        {
            return new FolderMonitorConfig
            {
                Path = this.Path,
                ShouldBeEmpty = this.ShouldBeEmpty,
                MonitorLastModified = this.MonitorLastModified,
                CheckZeroByteFiles = this.CheckZeroByteFiles
            };
        }

        public static ApiFolderMonitorConfigDto FromFolderMonitorConfig(FolderMonitorConfig config)
        {
            return new ApiFolderMonitorConfigDto
            {
                Path = config.Path,
                ShouldBeEmpty = config.ShouldBeEmpty,
                MonitorLastModified = config.MonitorLastModified,
                CheckZeroByteFiles = config.CheckZeroByteFiles
            };
        }
    }
} 