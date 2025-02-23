namespace SystemChecker.Core.Models;

public class DiskStatus
{
    public string Name { get; set; }
    public string Label { get; set; }
    public long TotalSpace { get; set; }
    public long FreeSpace { get; set; }
    public double UsagePercentage { get; set; }
} 