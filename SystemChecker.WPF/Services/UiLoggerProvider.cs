using Microsoft.Extensions.Logging;

namespace SystemChecker.WPF.Services
{
    public class UiLoggerProvider : ILoggerProvider
    {
        private readonly UiLoggerService _logger;

        public UiLoggerProvider()
        {
            _logger = new UiLoggerService("SystemChecker");
        }

        public ILogger CreateLogger(string categoryName) => _logger;

        public void Dispose() { }

        public UiLoggerService LoggerService => _logger;
    }
} 