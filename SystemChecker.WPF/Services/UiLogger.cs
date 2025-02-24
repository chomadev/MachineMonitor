using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using SystemChecker.WPF.Services;

public class UiLogger : ILogger
{
    private readonly string _categoryName;
    private readonly UiLoggerProvider _provider;

    public UiLogger(string categoryName, UiLoggerProvider provider)
    {
        _categoryName = categoryName;
        _provider = provider;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        if (exception != null)
        {
            message = $"{message} {exception}";
        }

        var entry = new LogEntry {
            Timestamp = DateTime.Now,
            Level = logLevel,
            Message = message
        };
        _provider.LoggerService.AddLog(entry);
        _provider.NotifySubscribers(entry);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }
} 