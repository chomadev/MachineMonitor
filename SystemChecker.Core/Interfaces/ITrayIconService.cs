namespace SystemChecker.Core.Interfaces;

public interface ITrayIconService
{
    void Initialize();
    void ShowNotification(string title, string message);
} 