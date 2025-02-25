using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SystemChecker.Core.Interfaces;
using Cronos;
using System;
using System.Threading;
using System.Threading.Tasks;
using SystemChecker.Core.Models;

namespace SystemChecker.Infrastructure.Services
{
    public class SchedulerExecutionService : BackgroundService
    {
        private readonly ILogger<SchedulerExecutionService> _logger;
        private readonly ISystemCheckService _systemCheckService;
        private readonly IConfigurationService _configService;
        private MachineConfiguration _currentConfig;
        private CronExpression? _cronExpression;

        public SchedulerExecutionService(
            ILogger<SchedulerExecutionService> logger,
            ISystemCheckService systemCheckService,
            IConfigurationService configService)
        {
            _logger = logger;
            _systemCheckService = systemCheckService;
            _configService = configService;
            _currentConfig = new MachineConfiguration();
            _cronExpression = CronExpression.Parse(_currentConfig.CheckSchedule);

            _configService.ConfigurationChanged += async (_, _) =>
            {
                var newConfig = await _configService.LoadConfigurationAsync();
                if (newConfig.CheckSchedule != _currentConfig.CheckSchedule)
                {
                    _currentConfig = newConfig;
                    _cronExpression = CronExpression.Parse(_currentConfig.CheckSchedule);
                    _logger.LogInformation(
                        "Schedule updated to: {Schedule}",
                        _currentConfig.CheckSchedule);
                }
            };

            // Carrega configuração inicial
            Task.Run(async () =>
            {
                try
                {
                    _currentConfig = await _configService.LoadConfigurationAsync();
                    _cronExpression = CronExpression.Parse(_currentConfig.CheckSchedule);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading initial configuration");
                }
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_cronExpression == null)
                    {
                        _logger.LogWarning("No valid CRON expression configured");
                        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                        continue;
                    }

                    var now = DateTime.UtcNow;
                    var nextRun = _cronExpression.GetNextOccurrence(now);

                    if (nextRun.HasValue)
                    {
                        var delay = nextRun.Value - now;
                        _logger.LogInformation(
                            "Next check scheduled for: {NextRun} (in {Delay})",
                            nextRun.Value.ToLocalTime(),
                            delay);

                        await Task.Delay(delay, stoppingToken);

                        var check = await _systemCheckService.PerformSystemCheckAsync();
                        await _systemCheckService.PushCheckResultAsync(check);
                    }
                    else
                    {
                        _logger.LogWarning("Could not determine next run time");
                        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in scheduler execution");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }
    }
}