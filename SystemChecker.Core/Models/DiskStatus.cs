namespace SystemChecker.Core.Models;

public class DiskStatus
{
    public string DriveLetter { get; set; } = string.Empty;
    public long TotalSpace { get; set; }
    public long FreeSpace { get; set; }
    public double UsagePercentage { get; set; }
} 