using System.Net.NetworkInformation;
using System.Threading.Tasks;
using SystemChecker.Core.Models;
using System.Linq;

namespace SystemChecker.Infrastructure.Services;

public interface INetworkChecker
{
    Task<NetworkStatus> CheckNetworkAsync();
}

public class NetworkChecker : INetworkChecker
{
    public async Task<NetworkStatus> CheckNetworkAsync()
    {
        var isConnected = NetworkInterface.GetIsNetworkAvailable();
        var hasInternet = false;

        if (isConnected)
        {
            try
            {
                using var ping = new Ping();
                var result = await ping.SendPingAsync("8.8.8.8", 3000);
                hasInternet = result.Status == IPStatus.Success;
            }
            catch
            {
                hasInternet = false;
            }
        }

        return new NetworkStatus
        {
            IsConnected = isConnected,
            HasInternetAccess = hasInternet,
            ActiveInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
                .Select(ni => ni.Name)
                .ToArray()
        };
    }
} 