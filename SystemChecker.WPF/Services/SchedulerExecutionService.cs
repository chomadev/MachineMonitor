using Cronos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SystemChecker.Core.Interfaces;
using SystemChecker.Core.Models;

namespace SystemChecker.WPF.Services
{
    public class SchedulerExecutionService : IHostedService, IDisposable
    {
        private readonly ISystemCheckService _systemCheckService;
        private readonly ITrayIconService _trayIconService;
        private readonly ISchedulerService _schedulerService;
        private readonly ILogger<SchedulerExecutionService> _logger;
        private Timer? _timer;
        private CronExpression _cronExpression;
        private DateTime? _nextRun;

        public event EventHandler<SystemCheck>? SystemCheckCompleted;
        public event EventHandler? SystemCheckStarted;

        public SchedulerExecutionService(
            ISystemCheckService systemCheckService,
            ITrayIconService trayIconService,
            ISchedulerService schedulerService,
            ILogger<SchedulerExecutionService> logger)
        {
            _systemCheckService = systemCheckService;
            _trayIconService = trayIconService;
            _schedulerService = schedulerService;
            _logger = logger;
            _cronExpression = CronExpression.Parse(_schedulerService.CurrentSchedule);

            // Subscribe to the schedule change event
            _schedulerService.ScheduleChanged += OnScheduleChanged;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting SchedulerExecutionService with CRON expression: {Schedule}",
                _schedulerService.CurrentSchedule);

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping SchedulerExecutionService");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private void OnScheduleChanged(object? sender, string newSchedule)
        {
            _cronExpression = CronExpression.Parse(newSchedule);
            _nextRun = null;
            _logger.LogInformation("Schedule updated to: {Schedule}", newSchedule);
        }

        private async void DoWork(object? state)
        {
            try
            {
                var utcNow = DateTime.UtcNow;

                if (_nextRun == null)
                {
                    _nextRun = _cronExpression.GetNextOccurrence(utcNow);
                    return;
                }

                if (utcNow >= _nextRun)
                {
                    await ExecuteSystemCheck();
                    _nextRun = _cronExpression.GetNextOccurrence(utcNow);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in scheduler processing");
            }
        }

        private async Task ExecuteSystemCheck()
        {
            try
            {
                _logger.LogInformation("Starting system check");
                
                // Notify the start of the check
                SystemCheckStarted?.Invoke(this, EventArgs.Empty);
                
                var result = await _systemCheckService.PerformSystemCheckAsync();
                await _systemCheckService.PushCheckResultAsync(result);
                
                SystemCheckCompleted?.Invoke(this, result);
                
                _trayIconService.ShowNotification(
                    "Scheduled Check",
                    "System check completed successfully.");
                
                _logger.LogInformation("System check completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during system check");
                _trayIconService.ShowNotification(
                    "Check Error",
                    $"An error occurred during the check: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _schedulerService.ScheduleChanged -= OnScheduleChanged;
        }
    }
}