using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SystemChecker.Core.Interfaces;
using SystemChecker.Core.Settings;

namespace SystemChecker.Infrastructure.Services
{
    public class SchedulerService : ISchedulerService
    {
        private readonly ILogger<SchedulerService> _logger;
        private readonly SchedulerSettings _settings;

        public string CurrentSchedule => _settings.CheckSchedule;
        public event EventHandler<string>? ScheduleChanged;

        public SchedulerService(
            IOptions<SchedulerSettings> settings,
            ILogger<SchedulerService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task UpdateScheduleAsync(string cronExpression)
        {
            _logger.LogInformation("Updating schedule to: {Schedule}", cronExpression);
            
            try
            {
                // Update the settings
                _settings.CheckSchedule = cronExpression;
                
                // Notify the change
                ScheduleChanged?.Invoke(this, cronExpression);
                
                _logger.LogInformation("Schedule updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating schedule");
                throw;
            }
        }
    }
} 