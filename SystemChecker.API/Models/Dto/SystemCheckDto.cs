namespace SystemChecker.API.Models.Dto;

public class SystemCheckDto
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }

    // System Status
    public ServiceStatusDto[] Services { get; set; } = Array.Empty<ServiceStatusDto>();
    public NetworkStatusDto? Network { get; set; }
    public DiskStatusDto[] Disks { get; set; } = Array.Empty<DiskStatusDto>();
    public CpuStatusDto? Cpu { get; set; }
    public MemoryStatusDto? Memory { get; set; }
    public TcpPortStatusDto[] Ports { get; set; } = Array.Empty<TcpPortStatusDto>();
    public FolderStatusDto[] Folders { get; set; } = Array.Empty<FolderStatusDto>();
    public FolderChangeDto[] FolderChanges { get; set; } = Array.Empty<FolderChangeDto>();
}

public class ServiceStatusDto
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsRunning { get; set; }
}

public class NetworkStatusDto
{
    public bool IsConnected { get; set; }
    public bool HasInternetAccess { get; set; }
    public string? IpAddress { get; set; }
    public string[] ActiveInterfaces { get; set; } = Array.Empty<string>();
    public MonitoredAddressDto[] MonitoredAddresses { get; set; } = Array.Empty<MonitoredAddressDto>();
}

public class MonitoredAddressDto
{
    public string Address { get; set; } = string.Empty;
    public bool IsReachable { get; set; }
    public int ResponseTime { get; set; }
}

public class DiskStatusDto
{
    public string Name { get; set; } = string.Empty;
    public long TotalSpace { get; set; }
    public long FreeSpace { get; set; }
    public double UsagePercentage { get; set; }
}

public class CpuStatusDto
{
    public double UsagePercentage { get; set; }
}

public class MemoryStatusDto
{
    public long TotalPhysicalMemory { get; set; }
    public long AvailablePhysicalMemory { get; set; }
    public double UsagePercentage { get; set; }
}

public class TcpPortStatusDto
{
    public int Port { get; set; }
    public bool IsOpen { get; set; }
    public string? Service { get; set; }
}

public class FolderStatusDto
{
    public string Path { get; set; } = string.Empty;
    public bool Exists { get; set; }
    public bool IsEmpty { get; set; }
    public DateTime? LastModified { get; set; }
    public bool HasZeroByteFiles { get; set; }
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> ZeroByteFiles { get; set; } = new();
}

public class FolderChangeDto
{
    public string Path { get; set; } = string.Empty;
    public DateTime LastChanged { get; set; }
    public string LastChangeType { get; set; } = string.Empty;
} 