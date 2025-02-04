namespace SystemChecker.Core.Settings
{
    public class SchedulerSettings
    {
        public string CheckSchedule { get; set; } = "*/5 * * * *"; // Default: every 5 minutes
    }
} 