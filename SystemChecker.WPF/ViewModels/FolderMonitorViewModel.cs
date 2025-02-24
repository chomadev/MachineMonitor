using SystemChecker.Core.Models;
using SystemChecker.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace SystemChecker.WPF.ViewModels;

public class FolderMonitorViewModel : ViewModelBase
{
    private readonly IMessagingCenter _messagingCenter;
    private readonly ILogger<FolderMonitorViewModel> _logger;
    private FolderStatus[] _folderStatuses = Array.Empty<FolderStatus>();
    private FolderChangeStatus[] _folderChanges = Array.Empty<FolderChangeStatus>();

    public FolderMonitorViewModel(
        IMessagingCenter messagingCenter,
        ILogger<FolderMonitorViewModel> logger)
    {
        _messagingCenter = messagingCenter;
        _logger = logger;

        // Subscribe to system check updates
        _messagingCenter.Subscribe<SystemCheck>(this, "SystemCheckCompleted", OnSystemCheckCompleted);
        _logger.LogInformation("FolderMonitorViewModel initialized and subscribed to SystemCheckCompleted");
    }

    public FolderStatus[] FolderStatuses
    {
        get => _folderStatuses;
        private set
        {
            _logger.LogInformation("Updating FolderStatuses - Count: {Count}", value?.Length ?? 0);
            SetField(ref _folderStatuses, value);
        }
    }

    public FolderChangeStatus[] FolderChanges
    {
        get => _folderChanges;
        private set
        {
            _logger.LogInformation("Updating FolderChanges - Count: {Count}", value?.Length ?? 0);
            SetField(ref _folderChanges, value);
        }
    }

    private void OnSystemCheckCompleted(SystemCheck systemCheck)
    {
        _logger.LogInformation("FolderMonitorViewModel received SystemCheckCompleted event");
        
        if (systemCheck.Folders != null)
        {
            FolderStatuses = systemCheck.Folders;
            _logger.LogInformation("Updated folder statuses: {Count} folders", systemCheck.Folders.Length);
        }
        
        if (systemCheck.FolderChanges != null)
        {
            FolderChanges = systemCheck.FolderChanges;
            _logger.LogInformation("Updated folder changes: {Count} changes", systemCheck.FolderChanges.Length);
            
            foreach (var change in systemCheck.FolderChanges.Take(5))
            {
                _logger.LogDebug("Recent change: {Path} - {Type} at {Time}", 
                    change.Path, 
                    change.LastChangeType, 
                    change.LastChanged);
            }
        }
    }
} 