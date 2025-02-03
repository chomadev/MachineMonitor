namespace SystemChecker.Core.Interfaces
{
    public interface ISchedulerService
    {
        string CurrentSchedule { get; }
        event EventHandler<string> ScheduleChanged;
        Task UpdateScheduleAsync(string cronExpression);
    }
} 