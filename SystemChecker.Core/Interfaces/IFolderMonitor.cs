using SystemChecker.Core.Models;

namespace SystemChecker.Core.Interfaces;

public interface IFolderMonitor
{
    Task<FolderStatus[]> CheckFoldersAsync(IEnumerable<FolderMonitorConfig> folders);
    FolderChangeStatus[] GetChanges();
} 