using System;
using Microsoft.Extensions.Logging;
using SystemChecker.WPF.Services;

namespace SystemChecker.WPF.ViewModels
{
    public class LogEntryViewModel : ViewModelBase
    {
        private readonly LogEntry _logEntry;
        private bool _isExpanded;

        public LogEntryViewModel(LogEntry logEntry)
        {
            _logEntry = logEntry;
        }

        public DateTime Timestamp => _logEntry.Timestamp;
        public LogLevel Level => _logEntry.Level;
        public string FullMessage => _logEntry.Message;

        public string ShortMessage
        {
            get
            {
                var message = _logEntry.Message.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
                return message.Length > 150 ? message.Substring(0, 150) + "..." : message;
            }
        }

        public bool HasMoreDetails => _logEntry.Message.Length > 150 || (_logEntry.Message.Contains('\r') || _logEntry.Message.Contains('\n'));

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetField(ref _isExpanded, value);
        }

        public LogEntry OriginalLogEntry => _logEntry;
    }
} 