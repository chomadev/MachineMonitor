namespace SystemChecker.Core.Models;

public class MachineConfiguration
{
    public string CheckSchedule { get; set; } = "*/5 * * * *";

    public List<string> ServicesToMonitor { get; set; } = new();

    public List<string> IpAddressesToMonitor { get; set; } = new();

    public List<int> TcpPorts { get; set; } = new();

    public List<FolderMonitorConfig> MonitoredFolders { get; set; } = new();

    public DateTime LastUpdated { get; set; }
} 