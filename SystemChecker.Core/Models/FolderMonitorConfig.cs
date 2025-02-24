namespace SystemChecker.Core.Models;

public class FolderMonitorConfig
{
    public string Path { get; set; } = string.Empty;
    public bool ShouldBeEmpty { get; set; }
    public bool MonitorLastModified { get; set; }
    public bool CheckZeroByteFiles { get; set; }
} 