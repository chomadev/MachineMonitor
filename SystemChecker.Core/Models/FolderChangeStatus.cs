namespace SystemChecker.Core.Models;

public class FolderChangeStatus
{
    public string Path { get; set; } = string.Empty;
    public DateTime LastChanged { get; set; }
    public string LastChangeType { get; set; } = string.Empty;
} 