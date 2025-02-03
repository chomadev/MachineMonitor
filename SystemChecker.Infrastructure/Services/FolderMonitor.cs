using System;
using System.Collections.Concurrent;
using System.IO;
using SystemChecker.Core.Models;
using Microsoft.Extensions.Options;
using SystemChecker.Infrastructure.Settings;

namespace SystemChecker.Infrastructure.Services;

public interface IFolderMonitor
{
    FolderChangeStatus[] GetChanges();
}

public class FolderMonitor : IFolderMonitor, IDisposable
{
    private readonly ConcurrentDictionary<string, FileSystemWatcher> _watchers;
    private readonly ConcurrentDictionary<string, FolderChangeStatus> _changes;
    private readonly FolderMonitorSettings _settings;

    public FolderMonitor(IOptions<FolderMonitorSettings> settings)
    {
        _settings = settings.Value;
        _watchers = new ConcurrentDictionary<string, FileSystemWatcher>();
        _changes = new ConcurrentDictionary<string, FolderChangeStatus>();
        InitializeWatchers();
    }

    private void InitializeWatchers()
    {
        foreach (var path in _settings.FoldersToMonitor)
        {
            if (Directory.Exists(path))
            {
                var watcher = new FileSystemWatcher(path)
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

                _watchers.TryAdd(path, watcher);
                _changes.TryAdd(path, new FolderChangeStatus
                {
                    Path = path,
                    LastChanged = DateTime.UtcNow,
                    LastChangeType = "Monitoring Started"
                });
            }
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
        return _changes.Values.ToArray();
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
} 