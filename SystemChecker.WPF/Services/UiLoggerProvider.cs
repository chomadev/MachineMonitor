using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace SystemChecker.WPF.Services
{
    public class UiLoggerProvider : ILoggerProvider
    {
        public UiLoggerService LoggerService { get; }
        private readonly List<Action<LogEntry>> _subscribers;

        public UiLoggerProvider()
        {
            LoggerService = new UiLoggerService();
            _subscribers = new List<Action<LogEntry>>();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new UiLogger(categoryName, this);
        }

        public IDisposable Subscribe(Action<LogEntry> onNewEntry)
        {
            _subscribers.Add(onNewEntry);
            return new SubscriptionToken(() => _subscribers.Remove(onNewEntry));
        }

        internal void NotifySubscribers(LogEntry entry)
        {
            foreach (var subscriber in _subscribers.ToList())
            {
                try
                {
                    subscriber(entry);
                }
                catch (Exception ex)
                {
                    // Log error but don't break other subscribers
                    LoggerService.AddLog(new LogEntry {
                        Timestamp = DateTime.Now,
                        Level = LogLevel.Error,
                        Message = $"Error in log subscriber: {ex.Message}"
                    });
                }
            }
        }

        public void Dispose()
        {
            _subscribers.Clear();
        }
    }

    internal class SubscriptionToken : IDisposable
    {
        private readonly Action _onDispose;

        public SubscriptionToken(Action onDispose)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            _onDispose();
        }
    }
} 