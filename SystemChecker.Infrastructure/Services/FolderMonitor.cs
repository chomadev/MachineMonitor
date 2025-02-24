using System;
using System.Collections.Concurrent;
using System.IO;
using SystemChecker.Core.Models;
using Microsoft.Extensions.Options;
using SystemChecker.Infrastructure.Settings;
using SystemChecker.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace SystemChecker.Infrastructure.Services;

public class FolderMonitor : IFolderMonitor, IDisposable
{
    private readonly ConcurrentDictionary<string, FileSystemWatcher> _watchers;
    private readonly ConcurrentDictionary<string, FolderChangeStatus> _changes;
    private readonly ILogger<FolderMonitor> _logger;
    private readonly IConfigurationService _configService;

    public FolderMonitor(
        ILogger<FolderMonitor> logger,
        IConfigurationService configService)
    {
        _watchers = new ConcurrentDictionary<string, FileSystemWatcher>();
        _changes = new ConcurrentDictionary<string, FolderChangeStatus>();
        _logger = logger;
        _configService = configService;
        
        // Subscribe to configuration changes
        _configService.ConfigurationChanged += (s, e) => UpdateWatchers();
        InitializeWatchers();
    }

    private void InitializeWatchers()
    {
        var folders = _configService.GetMonitoredFolders();
        foreach (var folder in folders)
        {
            if (!Directory.Exists(folder.Path)) continue;

            var watcher = new FileSystemWatcher(folder.Path)
            {
                NotifyFilter = NotifyFilters.DirectoryName
                              | NotifyFilters.FileName
                              | NotifyFilters.LastWrite
                              | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnRenamed;

            _watchers.TryAdd(folder.Path, watcher);
            _changes.TryAdd(folder.Path, new FolderChangeStatus
            {
                Path = folder.Path,
                LastChanged = DateTime.UtcNow,
                LastChangeType = "Monitoring Started"
            });
        }
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        _changes.AddOrUpdate(
            e.FullPath,
            new FolderChangeStatus
            {
                Path = e.FullPath,
                LastChanged = DateTime.UtcNow,
                LastChangeType = e.ChangeType.ToString()
            },
            (key, existing) => new FolderChangeStatus
            {
                Path = e.FullPath,
                LastChanged = DateTime.UtcNow,
                LastChangeType = e.ChangeType.ToString()
            });
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        _changes.AddOrUpdate(
            e.FullPath,
            new FolderChangeStatus
            {
                Path = e.FullPath,
                LastChanged = DateTime.UtcNow,
                LastChangeType = $"Renamed from {e.OldName}"
            },
            (key, existing) => new FolderChangeStatus
            {
                Path = e.FullPath,
                LastChanged = DateTime.UtcNow,
                LastChangeType = $"Renamed from {e.OldName}"
            });
    }

    public FolderChangeStatus[] GetChanges()
    {
        // Return the most recent changes first
        return _changes.Values
            .OrderByDescending(c => c.LastChanged)
            .ToArray();
    }

    public void Dispose()
    {
        foreach (var watcher in _watchers.Values)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }
        
        _watchers.Clear();
        _changes.Clear();
    }

    public async Task<FolderStatus[]> CheckFoldersAsync(IEnumerable<FolderMonitorConfig> folders)
    {
        var tasks = folders.Select(CheckFolderAsync);
        return await Task.WhenAll(tasks);
    }

    private async Task<FolderStatus> CheckFolderAsync(FolderMonitorConfig config)
    {
        var status = new FolderStatus { Path = config.Path };

        try
        {
            if (!Directory.Exists(config.Path))
            {
                status.ErrorMessage = "Directory does not exist";
                status.IsValid = false;
                return status;
            }

            status.Exists = true;
            var directory = new DirectoryInfo(config.Path);

            // Get all files (including subdirectories)
            var files = directory.GetFiles("*", SearchOption.AllDirectories);

            // Check if empty
            status.IsEmpty = !files.Any();

            // Check last modified
            if (config.MonitorLastModified && files.Any())
            {
                status.LastModified = files.Max(f => f.LastWriteTime);
            }

            // Check for zero byte files
            if (config.CheckZeroByteFiles)
            {
                var zeroByteFiles = files.Where(f => f.Length == 0)
                    .Select(f => f.FullName)
                    .ToList();

                status.HasZeroByteFiles = zeroByteFiles.Any();
                status.ZeroByteFiles = zeroByteFiles;
            }

            // Validate according to configuration
            status.IsValid = ValidateFolder(status, config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking folder {Path}", config.Path);
            status.ErrorMessage = ex.Message;
            status.IsValid = false;
        }

        return status;
    }

    private bool ValidateFolder(FolderStatus status, FolderMonitorConfig config)
    {
        if (!status.Exists) return false;

        if (config.ShouldBeEmpty && !status.IsEmpty)
        {
            status.ErrorMessage = "Directory should be empty but contains files";
            return false;
        }

        if (config.CheckZeroByteFiles && status.HasZeroByteFiles)
        {
            status.ErrorMessage = $"Directory contains {status.ZeroByteFiles.Count} zero-byte files";
            return false;
        }

        return true;
    }

    private void UpdateWatchers()
    {
        // Stop and dispose existing watchers
        foreach (var watcher in _watchers.Values)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }
        _watchers.Clear();
        
        // Initialize new watchers
        InitializeWatchers();
    }
} 