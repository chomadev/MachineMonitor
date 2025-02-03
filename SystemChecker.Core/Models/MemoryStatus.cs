namespace SystemChecker.Core.Models;

public class MemoryStatus
{
    public long TotalPhysicalMemory { get; set; }
    public long AvailablePhysicalMemory { get; set; }
    public double UsagePercentage { get; set; }
} 