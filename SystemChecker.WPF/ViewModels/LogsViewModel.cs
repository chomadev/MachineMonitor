using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SystemChecker.WPF.Commands;
using SystemChecker.WPF.Services;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SystemChecker.WPF.ViewModels
{
    public class LogsViewModel : ViewModelBase, IDisposable
    {
        private readonly UiLoggerService _loggerService;
        private LogLevel _selectedLogLevel;
        private ObservableCollection<LogEntry> _filteredLogs;
        private string _searchText = string.Empty;
        private readonly IDisposable _subscription;

        public LogsViewModel(UiLoggerProvider loggerProvider)
        {
            _loggerService = loggerProvider.LoggerService;
            _selectedLogLevel = LogLevel.Information;
            _filteredLogs = new ObservableCollection<LogEntry>();
            
            ClearLogsCommand = new RelayCommand(ClearLogs);

            // Insert the event handler for the logs collection change
            _loggerService.Logs.CollectionChanged += OnLogsCollectionChanged;
            
            // Load the initial logs
            UpdateFilteredLogs();

            // Subscribe to new log entries
            _subscription = loggerProvider.Subscribe(OnNewLogEntry);
        }

        private void OnLogsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateFilteredLogs();
        }

        private void UpdateFilteredLogs()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                FilteredLogs.Clear();
                var filtered = _loggerService.Logs.Where(l => l.Level >= SelectedLogLevel).ToList();
                foreach (var log in filtered)
                {
                    FilteredLogs.Add(log);
                }
                OnPropertyChanged(nameof(FilteredLogs));
            });
        }

        public ObservableCollection<LogEntry> FilteredLogs
        {
            get => _filteredLogs;
            private set => SetField(ref _filteredLogs, value);
        }

        public LogLevel[] LogLevels => new[]
        {
            LogLevel.Trace,
            LogLevel.Debug,
            LogLevel.Information,
            LogLevel.Warning,
            LogLevel.Error,
            LogLevel.Critical
        };

        public LogLevel SelectedLogLevel
        {
            get => _selectedLogLevel;
            set
            {
                if (SetField(ref _selectedLogLevel, value))
                {
                    UpdateFilteredLogs();
                }
            }
        }

        public ICommand ClearLogsCommand { get; }

        private void ClearLogs()
        {
            _loggerService.Logs.Clear();
            FilteredLogs.Clear();
            OnPropertyChanged(nameof(FilteredLogs));
        }

        private void OnNewLogEntry(LogEntry entry)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _loggerService.Logs.Add(entry);
                if (string.IsNullOrWhiteSpace(SearchText) || 
                    entry.Message.ToLower().Contains(SearchText.ToLower()))
                {
                    FilteredLogs.Add(entry);
                }
                
                // Keep only last 1000 entries
                while (_loggerService.Logs.Count > 1000)
                {
                    _loggerService.Logs.RemoveAt(0);
                    if (FilteredLogs.Count > 0)
                    {
                        FilteredLogs.RemoveAt(0);
                    }
                }
            });
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetField(ref _searchText, value))
                {
                    FilterLogs();
                }
            }
        }

        private void FilterLogs()
        {
            FilteredLogs.Clear();
            
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var log in _loggerService.Logs)
                {
                    FilteredLogs.Add(log);
                }
                return;
            }

            var searchTerms = SearchText.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var log in _loggerService.Logs)
            {
                if (searchTerms.All(term => 
                    log.Message.ToLower().Contains(term) || 
                    log.Level.ToString().ToLower().Contains(term) ||
                    log.Timestamp.ToString("yyyy/MM/dd HH:mm:ss").Contains(term)))
                {
                    FilteredLogs.Add(log);
                }
            }
        }

        public void Dispose()
        {
            _loggerService.Logs.CollectionChanged -= OnLogsCollectionChanged;
            _subscription.Dispose();
        }
    }
} 