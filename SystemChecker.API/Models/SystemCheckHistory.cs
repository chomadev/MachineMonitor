using System.Text.Json.Serialization;

namespace SystemChecker.API.Models;

public class SystemCheckHistory
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public int MachineId { get; set; }
    public Machine Machine { get; set; } = null!;

    // Dados do sistema
    public ICollection<ServiceStatus> Services { get; set; } = new List<ServiceStatus>();
    public NetworkStatus? Network { get; set; }
    public ICollection<DiskStatus> Disks { get; set; } = new List<DiskStatus>();
    public CpuStatus? Cpu { get; set; }
    public MemoryStatus? Memory { get; set; }
    public ICollection<TcpPortStatus> Ports { get; set; } = new List<TcpPortStatus>();
    public ICollection<FolderStatus> Folders { get; set; } = new List<FolderStatus>();
    public ICollection<FolderChange> FolderChanges { get; set; } = new List<FolderChange>();
    public ICollection<MonitoredAddress> MonitoredAddresses { get; set; } = new List<MonitoredAddress>();
}
public class ApiKey
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsed { get; set; }
    
    public int MachineId { get; set; }
    public Machine Machine { get; set; } = null!;
} 
public class Machine
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    [JsonIgnore]
    public ApiKey? ApiKey { get; set; }
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

public class FolderChange
{
    public int Id { get; set; }
    public string Path { get; set; } = string.Empty;
    public DateTime LastChanged { get; set; }
    public string LastChangeType { get; set; } = string.Empty;
    public int SystemCheckHistoryId { get; set; }
    [JsonIgnore] public SystemCheckHistory SystemCheckHistory { get; set; } = null!;
} 
public class FolderStatus
{
    public int Id { get; set; }
    public string Path { get; set; } = string.Empty;
    public bool Exists { get; set; }
    public bool IsEmpty { get; set; }
    public DateTime? LastModified { get; set; }
    public bool HasZeroByteFiles { get; set; }
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string ZeroByteFiles { get; set; } = string.Empty; // JSON serialized
    public int SystemCheckHistoryId { get; set; }
    [JsonIgnore] public SystemCheckHistory SystemCheckHistory { get; set; } = null!;
} 
public class MonitoredAddress
{
    public int Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public bool IsReachable { get; set; }
    public int ResponseTime { get; set; }
    public int SystemCheckHistoryId { get; set; }
    [JsonIgnore] public SystemCheckHistory SystemCheckHistory { get; set; } = null!;
} 

public class NetworkStatus
{
    public int Id { get; set; }
    public bool IsConnected { get; set; }
    public bool HasInternetAccess { get; set; }
    public string? IpAddress { get; set; }
    public string ActiveInterfaces { get; set; } = string.Empty; // JSON serialized
    public int SystemCheckHistoryId { get; set; }
    [JsonIgnore] public SystemCheckHistory SystemCheckHistory { get; set; } = null!;
} 
