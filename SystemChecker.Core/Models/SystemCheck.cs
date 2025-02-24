using System;

namespace SystemChecker.Core.Models;

public class SystemCheck
{
    public SystemCheck()
    {
        Services = Array.Empty<ServiceStatus>();
        Network = new NetworkStatus 
        { 
            ActiveInterfaces = Array.Empty<string>() 
        };
        Disks = Array.Empty<DiskStatus>();
        Cpu = new CpuStatus();
        Memory = new MemoryStatus();
        Ports = Array.Empty<TcpPortStatus>();
        FolderChanges = Array.Empty<FolderChangeStatus>();
    }

    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public ServiceStatus[] Services { get; set; }
    public NetworkStatus Network { get; set; }
    public DiskStatus[] Disks { get; set; }
    public CpuStatus Cpu { get; set; }
    public MemoryStatus Memory { get; set; }
    public TcpPortStatus[] Ports { get; set; }
    public FolderChangeStatus[] FolderChanges { get; set; }
    public FolderStatus[]? Folders { get; set; }
} 