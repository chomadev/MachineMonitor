namespace SystemChecker.Core.Models;

public class TcpPortStatus
{
    public int Port { get; set; }
    public bool IsOpen { get; set; }
    public string Service { get; set; } = string.Empty;
} 