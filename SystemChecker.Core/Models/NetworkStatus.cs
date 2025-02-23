namespace SystemChecker.Core.Models;

public class NetworkStatus
{
    public NetworkStatus()
    {
        ActiveInterfaces = Array.Empty<string>();
    }

    public bool IsConnected { get; set; }
    public bool HasInternetAccess { get; set; }
    public string? IpAddress { get; set; }
    public string[] ActiveInterfaces { get; set; } = Array.Empty<string>();
    public IpAddressStatus[] MonitoredAddresses { get; set; } = Array.Empty<IpAddressStatus>();
} 