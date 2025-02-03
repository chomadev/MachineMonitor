namespace SystemChecker.Infrastructure.Settings;

public class TcpPortSettings
{
    public PortConfig[] Ports { get; set; } = Array.Empty<PortConfig>();
}

public class PortConfig
{
    public int Port { get; set; }
    public string ServiceName { get; set; } = string.Empty;
} 