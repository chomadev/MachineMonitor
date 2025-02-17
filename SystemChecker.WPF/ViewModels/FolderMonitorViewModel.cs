using System.Collections.ObjectModel;
using System.Windows.Forms;
using SystemChecker.Core.Interfaces;
using SystemChecker.Core.Models;
using SystemChecker.WPF.Commands;

namespace SystemChecker.WPF.ViewModels;

public class FolderMonitorViewModel : ViewModelBase
{
    private readonly IConfigurationService _configService;
    private readonly IFolderMonitor _folderMonitor;
    private string _selectedFolder;
    private bool _includeSubdirectories;
    private string _fileExtensions;
    private ObservableCollection<FolderChangeStatus> _monitoredFolders;

    public FolderMonitorViewModel(
        IConfigurationService configService,
        IFolderMonitor folderMonitor)
    {
        _configService = configService;
        _folderMonitor = folderMonitor;
        _monitoredFolders = new ObservableCollection<FolderChangeStatus>();

        AddFolderCommand = new RelayCommand(AddFolder);
        RemoveFolderCommand = new RelayCommand(RemoveFolder, CanRemoveFolder);
        SaveCommand = new AsyncRelayCommand(SaveChanges);

        LoadConfiguration();
    }

    public ObservableCollection<FolderChangeStatus> MonitoredFolders
    {
        get => _monitoredFolders;
        set => SetField(ref _monitoredFolders, value);
    }

    public string SelectedFolder
    {
        get => _selectedFolder;
        set
        {
            if (SetField(ref _selectedFolder, value))
            {
                (RemoveFolderCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IncludeSubdirectories
    {
        get => _includeSubdirectories;
        set => SetField(ref _includeSubdirectories, value);
    }

    public string FileExtensions
    {
        get => _fileExtensions;
        set => SetField(ref _fileExtensions, value);
    }

    public ICommand AddFolderCommand { get; }
    public ICommand RemoveFolderCommand { get; }
    public ICommand SaveCommand { get; }

    private void LoadConfiguration()
    {
        var changes = _folderMonitor.GetChanges();
        MonitoredFolders = new ObservableCollection<FolderChangeStatus>(changes);
        
        var settings = _configService.GetFolderMonitorSettings();
        IncludeSubdirectories = settings.IncludeSubdirectories;
        FileExtensions = string.Join(",", settings.FileExtensionsToMonitor);
    }

    private void AddFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select a folder to monitor",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            var newFolder = new FolderChangeStatus
            {
                Path = dialog.SelectedPath,
                LastChanged = DateTime.Now,
                LastChangeType = "Added for monitoring"
            };

            if (!MonitoredFolders.Any(f => f.Path == newFolder.Path))
            {
                MonitoredFolders.Add(newFolder);
            }
        }
    }

    private bool CanRemoveFolder()
    {
        return !string.IsNullOrEmpty(SelectedFolder);
    }

    private void RemoveFolder()
    {
        if (CanRemoveFolder())
        {
            var folder = MonitoredFolders.FirstOrDefault(f => f.Path == SelectedFolder);
            if (folder != null)
            {
                MonitoredFolders.Remove(folder);
            }
        }
    }

    private async Task SaveChanges()
    {
        var settings = new FolderMonitorSettings
        {
            FoldersToMonitor = MonitoredFolders.Select(f => f.Path).ToList(),
            IncludeSubdirectories = IncludeSubdirectories,
            FileExtensionsToMonitor = FileExtensions.Split(',')
                .Select(ext => ext.Trim())
                .Where(ext => !string.IsNullOrEmpty(ext))
                .Select(ext => ext.StartsWith(".") ? ext : $".{ext}")
                .ToArray()
        };

        await _configService.UpdateFolderMonitorSettingsAsync(settings);
    }
} 