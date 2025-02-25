// namespace SystemChecker.API.Models;

// public class SystemCheckData
// {
//     public Guid Id { get; set; }
//     public DateTime Timestamp { get; set; }
//     public List<ServiceStatusData> Services { get; set; } = new();
//     public NetworkStatusData? Network { get; set; }
//     public List<DiskStatusData> Disks { get; set; } = new();
//     public CpuStatusData? Cpu { get; set; }
//     public MemoryStatusData? Memory { get; set; }
//     public List<TcpPortStatusData> Ports { get; set; } = new();
//     public List<FolderChangeData> FolderChanges { get; set; } = new();
//     public List<FolderStatusData> Folders { get; set; } = new();
// }
// public class MemoryStatusData
// {
//     public int Id { get; set; }
//     public long TotalPhysicalMemory { get; set; }
//     public long AvailablePhysicalMemory { get; set; }
//     public double UsagePercentage { get; set; }
// }
// public class CpuStatusData
// {
//     public int Id { get; set; }
//     public double UsagePercentage { get; set; }
//     public int ProcessCount { get; set; }
// } 

// public class ServiceStatusData
// {
//     public string Name { get; set; } = string.Empty;
//     public string DisplayName { get; set; } = string.Empty;
//     public string Status { get; set; } = string.Empty;
//     public bool IsRunning { get; set; }
// }

// public class NetworkStatusData
// {
//     public bool IsConnected { get; set; }
//     public bool HasInternetAccess { get; set; }
//     public string? IpAddress { get; set; }
//     public List<string> ActiveInterfaces { get; set; } = new();
//     public List<MonitoredAddressData> MonitoredAddresses { get; set; } = new();
// }

// public class MonitoredAddressData
// {
//     public string Address { get; set; } = string.Empty;
//     public bool IsReachable { get; set; }
//     public int ResponseTime { get; set; }
// } 

// public class FolderStatusData
// {
//     public int Id { get; set; }
//     public string Path { get; set; } = string.Empty;
//     public bool Exists { get; set; }
//     public bool IsEmpty { get; set; }
//     public DateTime? LastModified { get; set; }
//     public bool HasZeroByteFiles { get; set; }
//     public bool IsValid { get; set; }
//     public string? ErrorMessage { get; set; }
//     public List<string> ZeroByteFiles { get; set; } = new();
// }

// public class FolderChangeData
// {
//     public int Id { get; set; }
//     public string Path { get; set; } = string.Empty;
//     public DateTime LastChanged { get; set; }
//     public string LastChangeType { get; set; } = string.Empty;
// } 

// public class DiskStatusData
// {
//     public int Id { get; set; }
//     public string Name { get; set; } = string.Empty;
//     public long TotalSpace { get; set; }
//     public long FreeSpace { get; set; }
//     public double UsagePercentage { get; set; }
// }
// public class TcpPortStatusData
// {
//     public int Port { get; set; }
//     public bool IsOpen { get; set; }
//     public string? Service { get; set; }
// } 

// public class MachineConfigurationData
// {
//     public Machine Machine { get; set; } = null!;
    
//     public string CheckSchedule { get; set; } = "*/5 * * * *";

//     public List<string> ServicesToMonitor { get; set; } = new();

//     public List<string> IpAddressesToMonitor { get; set; } = new();

//     public List<int> TcpPorts { get; set; } = new();

//     public List<FolderMonitorConfigData> MonitoredFolders { get; set; } = new();

//     public DateTime LastUpdated { get; set; }
// }

// public class FolderMonitorConfigData
// {
//     public string Path { get; set; } = string.Empty;
//     public bool ShouldBeEmpty { get; set; }
//     public bool MonitorLastModified { get; set; }
//     public bool CheckZeroByteFiles { get; set; }
// } 