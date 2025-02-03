namespace SystemChecker.Core.Models;

public class ServiceStatus
{
    public string Name { get; set; } = string.Empty;
    public bool IsRunning { get; set; }
    public string Status { get; set; } = string.Empty;
} 