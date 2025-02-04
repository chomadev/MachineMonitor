namespace SystemChecker.API.Models;

public class SystemCheckData
{
    public DateTime Timestamp { get; set; }
    public List<ServiceStatusData> Services { get; set; } = new();
    public NetworkStatusData? Network { get; set; }
    public List<DiskStatusData> Disks { get; set; } = new();
    public CpuStatusData? Cpu { get; set; }
    public MemoryStatusData? Memory { get; set; }
    public List<TcpPortStatusData> Ports { get; set; } = new();
}

public class ServiceStatusData
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsRunning { get; set; }
}

public class NetworkStatusData
{
    public bool IsConnected { get; set; }
    public bool HasInternetAccess { get; set; }
    public string? IpAddress { get; set; }
}

public class DiskStatusData
{
    public string Name { get; set; } = string.Empty;
    public long TotalSpace { get; set; }
    public long FreeSpace { get; set; }
    public double UsagePercentage { get; set; }
}

public class CpuStatusData
{
    public double UsagePercentage { get; set; }
}

public class MemoryStatusData
{
    public long TotalPhysicalMemory { get; set; }
    public long AvailablePhysicalMemory { get; set; }
    public double UsagePercentage { get; set; }
}

public class TcpPortStatusData
{
    public int Port { get; set; }
    public bool IsOpen { get; set; }
    public string? Service { get; set; }
} 