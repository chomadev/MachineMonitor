namespace SystemChecker.Infrastructure.Settings;

public class FolderMonitorSettings
{
    public List<string> FoldersToMonitor { get; set; } = new();
    public string[] FileExtensionsToMonitor { get; set; } = new[] { ".txt", ".log", ".json", ".xml" };
    public bool IncludeSubdirectories { get; set; } = true;
} 