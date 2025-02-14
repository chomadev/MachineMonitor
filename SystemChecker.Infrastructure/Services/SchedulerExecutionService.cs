using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SystemChecker.Core.Interfaces;
using Cronos;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SystemChecker.Infrastructure.Services
{
    public class SchedulerExecutionService : BackgroundService
    {
        private readonly ILogger<SchedulerExecutionService> _logger;
        private readonly ISystemCheckService _systemCheckService;
        private readonly IMessagingCenter _messagingCenter;
        private readonly IConfigurationService _configService;

        public SchedulerExecutionService(
            ILogger<SchedulerExecutionService> logger,
            ISystemCheckService systemCheckService,
            IMessagingCenter messagingCenter,
            IConfigurationService configService)
        {
            _logger = logger;
            _systemCheckService = systemCheckService;
            _messagingCenter = messagingCenter;
            _configService = configService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var systemCheck = await _systemCheckService.PerformSystemCheckAsync();
                    _messagingCenter.Publish(systemCheck, "SystemCheckCompleted");
                    await _systemCheckService.PushCheckResultAsync(systemCheck);

                    await WaitForNextSchedule(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing scheduled check");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }

        private async Task WaitForNextSchedule(CancellationToken stoppingToken)
        {
            try
            {
                var cronExpression = CronExpression.Parse(_configService.GetCurrentSchedule());
                var nextRun = cronExpression.GetNextOccurrence(DateTime.UtcNow);
                
                if (nextRun.HasValue)
                {
                    var delay = nextRun.Value - DateTime.UtcNow;
                    if (delay > TimeSpan.Zero)
                    {
                        _logger.LogInformation("Next check scheduled for: {nextRun}", nextRun.Value.ToLocalTime());
                        await Task.Delay(delay, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating next schedule");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}