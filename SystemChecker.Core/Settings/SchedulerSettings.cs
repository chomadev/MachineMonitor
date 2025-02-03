namespace SystemChecker.Core.Settings
{
    public class SchedulerSettings
    {
        public string CheckSchedule { get; set; } = "*/5 * * * *"; // Padrão: a cada 5 minutos
    }
} 