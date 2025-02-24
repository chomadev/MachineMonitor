namespace SystemChecker.Core.Models;

public class FolderStatus
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