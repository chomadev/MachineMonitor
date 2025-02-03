using System.Text.Json.Serialization;

namespace SystemChecker.API.Models;

public class SystemCheckHistory
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public int MachineId { get; set; }
    [JsonIgnore] public Machine Machine { get; set; } = null!;

    // Dados do sistema
    public List<ServiceStatus> Services { get; set; } = new();
    public NetworkStatus? Network { get; set; }
    public List<DiskStatus> Disks { get; set; } = new();
    public CpuStatus? Cpu { get; set; }
    public MemoryStatus? Memory { get; set; }
    public List<TcpPortStatus> Ports { get; set; } = new();
}

public class ServiceStatus
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsRunning { get; set; }
    
    public int SystemCheckHistoryId { get; set; }
    [JsonIgnore] public SystemCheckHistory SystemCheckHistory { get; set; } = null!;
}

public class NetworkStatus
{
    public int Id { get; set; }
    public bool IsConnected { get; set; }
    public bool HasInternetAccess { get; set; }
    public string? IpAddress { get; set; }
    
    public int SystemCheckHistoryId { get; set; }
    [JsonIgnore] public SystemCheckHistory SystemCheckHistory { get; set; } = null!;
}

public class DiskStatus
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long TotalSpace { get; set; }
    public long FreeSpace { get; set; }
    public double UsagePercentage { get; set; }
    
    public int SystemCheckHistoryId { get; set; }
    [JsonIgnore] public SystemCheckHistory SystemCheckHistory { get; set; } = null!;
}

public class CpuStatus
{
    public int Id { get; set; }
    public double UsagePercentage { get; set; }
    
    public int SystemCheckHistoryId { get; set; }
    [JsonIgnore] public SystemCheckHistory SystemCheckHistory { get; set; } = null!;
}

public class MemoryStatus
{
    public int Id { get; set; }
    public long TotalPhysicalMemory { get; set; }
    public long AvailablePhysicalMemory { get; set; }
    public double UsagePercentage { get; set; }
    
    public int SystemCheckHistoryId { get; set; }
    [JsonIgnore] public SystemCheckHistory SystemCheckHistory { get; set; } = null!;
}

public class TcpPortStatus
{
    public int Id { get; set; }
    public int Port { get; set; }
    public bool IsOpen { get; set; }
    public string? Service { get; set; }
    
    public int SystemCheckHistoryId { get; set; }
    [JsonIgnore] public SystemCheckHistory SystemCheckHistory { get; set; } = null!;
} 