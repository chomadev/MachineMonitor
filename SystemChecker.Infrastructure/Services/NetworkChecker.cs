using System.Net.NetworkInformation;
using System.Threading.Tasks;
using SystemChecker.Core.Models;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using SystemChecker.Core.Interfaces;

namespace SystemChecker.Infrastructure.Services;

public interface INetworkChecker
{
    Task<NetworkStatus> CheckNetworkAsync();
}

public class NetworkChecker : INetworkChecker
{
    private readonly IConfigurationService _configService;
    private readonly ILogger<NetworkChecker> _logger;
    private MachineConfiguration _currentConfig;

    public NetworkChecker(
        IConfigurationService configService,
        ILogger<NetworkChecker> logger)
    {
        _configService = configService;
        _logger = logger;
        _currentConfig = new MachineConfiguration();

        _configService.ConfigurationChanged += async (_, _) =>
        {
            _currentConfig = await _configService.LoadConfigurationAsync();
            _logger.LogInformation("Network monitoring configuration updated");
        };

        // Carrega configuração inicial
        Task.Run(async () =>
        {
            try
            {
                _currentConfig = await _configService.LoadConfigurationAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading initial configuration");
            }
        });
    }

    public async Task<NetworkStatus> CheckNetworkAsync()
    {
        var networkStatus = new NetworkStatus
        {
            IsConnected = NetworkInterface.GetIsNetworkAvailable(),
            HasInternetAccess = await CheckInternetAccessAsync(),
            IpAddress = GetLocalIPAddress(),
            ActiveInterfaces = GetActiveInterfaces()
        };

        // Check monitored IPs in parallel
        var tasks = _currentConfig.IpAddressesToMonitor.Select(ip => CheckIpAddressAsync(ip));
        networkStatus.MonitoredAddresses = (await Task.WhenAll(tasks)).ToArray();

        return networkStatus;
    }

    private async Task<IpAddressStatus> CheckIpAddressAsync(string address)
    {
        var status = new IpAddressStatus { Address = address };
        try
        {
            using var ping = new Ping();
            var stopwatch = Stopwatch.StartNew();
            var reply = await ping.SendPingAsync(address, 1000);
            stopwatch.Stop();

            status.IsReachable = reply.Status == IPStatus.Success;
            status.ResponseTime = stopwatch.ElapsedMilliseconds;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error pinging address {Address}", address);
            status.IsReachable = false;
            status.ResponseTime = -1;
        }
        return status;
    }

    private string GetLocalIPAddress()
    {
        try
        {
            var ip = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
                .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                .Where(addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                    && !System.Net.IPAddress.IsLoopback(addr.Address))
                .Select(addr => addr.Address.ToString())
                .FirstOrDefault();

            return ip ?? "Not Available";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting local IP address");
            return "Error";
        }
    }

    private string[] GetActiveInterfaces()
    {
        try
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
                .Select(ni => ni.Name)
                .ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active network interfaces");
            return Array.Empty<string>();
        }
    }

    private async Task<bool> CheckInternetAccessAsync()
    {
        try
        {
            using var ping = new Ping();
            var result = await ping.SendPingAsync("8.8.8.8", 3000);
            return result.Status == IPStatus.Success;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking internet access");
            return false;
        }
    }
} 