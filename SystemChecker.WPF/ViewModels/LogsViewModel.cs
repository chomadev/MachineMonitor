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
        private ObservableCollection<LogEntryViewModel> _filteredLogs;
        private string _searchText = string.Empty;
        private readonly IDisposable _subscription;

        public LogsViewModel(UiLoggerProvider loggerProvider)
        {
            _loggerService = loggerProvider.LoggerService;
            _selectedLogLevel = LogLevel.Information;
            _filteredLogs = new ObservableCollection<LogEntryViewModel>();
            
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
                    FilteredLogs.Add(new LogEntryViewModel(log));
                }
                OnPropertyChanged(nameof(FilteredLogs));
            });
        }

        public ObservableCollection<LogEntryViewModel> FilteredLogs
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
                // The UILoggerService already adds the log, so we just need to update the view model collection
                var logViewModel = new LogEntryViewModel(entry);
                if (string.IsNullOrWhiteSpace(SearchText) || 
                    logViewModel.FullMessage.ToLower().Contains(SearchText.ToLower()))
                {
                    FilteredLogs.Add(logViewModel);
                }
                
                // Keep only last 1000 entries
                if (FilteredLogs.Count > 1000)
                {
                    FilteredLogs.RemoveAt(0);
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
            
            var logsToFilter = _loggerService.Logs
                .Where(l => l.Level >= SelectedLogLevel)
                .AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchTerms = SearchText.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                logsToFilter = logsToFilter.Where(log =>
                    searchTerms.All(term =>
                        log.Message.ToLower().Contains(term) ||
                        log.Level.ToString().ToLower().Contains(term) ||
                        log.Timestamp.ToString("yyyy/MM/dd HH:mm:ss").Contains(term)));
            }

            foreach (var log in logsToFilter)
            {
                FilteredLogs.Add(new LogEntryViewModel(log));
            }
            OnPropertyChanged(nameof(FilteredLogs));
        }

        public void Dispose()
        {
            _loggerService.Logs.CollectionChanged -= OnLogsCollectionChanged;
            _subscription.Dispose();
        }
    }
} 