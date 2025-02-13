using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SystemChecker.WPF.Commands;
using SystemChecker.WPF.Services;

namespace SystemChecker.WPF.ViewModels
{
    public class LogsViewModel : ViewModelBase
    {
        private readonly UiLoggerService _loggerService;
        private LogLevel _selectedLogLevel;
        private ObservableCollection<LogEntry> _filteredLogs;

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

        public void Dispose()
        {
            _loggerService.Logs.CollectionChanged -= OnLogsCollectionChanged;
        }
    }
} 