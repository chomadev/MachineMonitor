using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace SystemChecker.WPF.Services
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public Exception? Exception { get; set; }
    }

    public class UiLoggerService : ILogger
    {
        private readonly ObservableCollection<LogEntry> _logs;
        private const int MaxLogEntries = 1000;

        public UiLoggerService()
        {
            _logs = new ObservableCollection<LogEntry>();
        }

        public ObservableCollection<LogEntry> Logs => _logs;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                var entry = new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Level = logLevel,
                    Message = formatter(state, exception),
                    Exception = exception
                };

                AddLog(entry);
            });
        }

        public void AddLog(LogEntry entry)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _logs.Insert(0, entry);
                
                while (_logs.Count > MaxLogEntries)
                {
                    _logs.RemoveAt(_logs.Count - 1);
                }
            });
        }
    }
} 