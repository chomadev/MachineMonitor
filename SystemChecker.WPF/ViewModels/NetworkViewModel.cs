using SystemChecker.Core.Models;
using SystemChecker.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace SystemChecker.WPF.ViewModels;

public class NetworkViewModel : ViewModelBase
{
    private readonly IMessagingCenter _messagingCenter;
    private readonly ILogger<NetworkViewModel> _logger;
    private NetworkStatus _networkStatus;

    public NetworkViewModel(
        IMessagingCenter messagingCenter,
        ILogger<NetworkViewModel> logger)
    {
        _messagingCenter = messagingCenter;
        _logger = logger;
        _networkStatus = new NetworkStatus();

        // Subscribe to system check updates
        _messagingCenter.Subscribe<SystemCheck>(this, "SystemCheckCompleted", OnSystemCheckCompleted);
        _logger.LogInformation("NetworkViewModel initialized and subscribed to SystemCheckCompleted");
    }

    public NetworkStatus NetworkStatus
    {
        get => _networkStatus;
        private set
        {
            _logger.LogInformation("Setting NetworkStatus - Old: {Old}, New: {New}", 
                _networkStatus?.IsConnected, value?.IsConnected);
            SetField(ref _networkStatus, value);
            OnPropertyChanged(nameof(NetworkStatus));  // Força atualização da UI
        }
    }

    private void OnSystemCheckCompleted(SystemCheck systemCheck)
    {
        _logger.LogInformation("NetworkViewModel received SystemCheckCompleted event");
        if (systemCheck.Network != null)
        {
            _logger.LogInformation("Updating NetworkStatus: Connected={Connected}, HasInternet={HasInternet}, IPs={IpCount}", 
                systemCheck.Network.IsConnected,
                systemCheck.Network.HasInternetAccess,
                systemCheck.Network.MonitoredAddresses?.Length ?? 0);
            NetworkStatus = systemCheck.Network;
        }
        else
        {
            _logger.LogWarning("Received null Network status in SystemCheck");
        }
    }
} 