using System.Windows.Input;
using SystemChecker.Core.Interfaces;
using SystemChecker.Core.Models;
using SystemChecker.WPF.Commands;

namespace SystemChecker.WPF.ViewModels;

public class MainViewModel : ViewModelBase, IDisposable
{
    private readonly ISystemCheckService _systemCheckService;
    private readonly IMessagingCenter _messagingCenter;
    private readonly IConfigurationService _configService;
    private SystemCheck? _lastCheck;
    private bool _isChecking;
    private ConfigurationViewModel _configViewModel;
    private string _lastCheckTime;
    private readonly TcpPortsViewModel _tcpPortsViewModel;
    private readonly ServicesViewModel _servicesViewModel;
    private readonly SystemResourcesViewModel _systemResourcesViewModel;
    private readonly NetworkViewModel _networkViewModel;
    private readonly LogsViewModel _logsViewModel;

    public MainViewModel(
        ISystemCheckService systemCheckService,
        IMessagingCenter messagingCenter,
        IConfigurationService configService,
        ConfigurationViewModel configViewModel,
        TcpPortsViewModel tcpPortsViewModel,
        ServicesViewModel servicesViewModel,
        NetworkViewModel networkViewModel,
        SystemResourcesViewModel systemResourcesViewModel,
        LogsViewModel logsViewModel)
    {
        _systemCheckService = systemCheckService;
        _messagingCenter = messagingCenter;
        _configService = configService;
        _configViewModel = configViewModel;
        _lastCheckTime = "None check performed";
        _tcpPortsViewModel = tcpPortsViewModel;
        _servicesViewModel = servicesViewModel;
        _systemResourcesViewModel = systemResourcesViewModel;
        _networkViewModel = networkViewModel;
        _logsViewModel = logsViewModel;

        CheckNowCommand = new AsyncRelayCommand(PerformCheck, () => !IsChecking);

        // Subscribe to messages instead of direct events
        _messagingCenter.Subscribe<SystemCheck>(this, "SystemCheckCompleted", OnSystemCheckCompleted);
        _messagingCenter.Subscribe<object>(this, "SystemCheckStarted", _ => OnSystemCheckStarted());

        // Subscribe to configuration changes
        _configService.ConfigurationChanged += async (s, e) =>
        {
            // Força um novo check quando a configuração mudar
            await PerformCheck();
        };
    }

    public ConfigurationViewModel ConfigViewModel
    {
        get => _configViewModel;
        private set => SetField(ref _configViewModel, value);
    }

    public SystemCheck? LastCheck
    {
        get => _lastCheck;
        private set
        {
            if (SetField(ref _lastCheck, value))
            {
                if (value != null)
                {
                    LastCheckTime = value.Timestamp.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");
                }
            }
        }
    }

    public bool IsChecking
    {
        get => _isChecking;
        private set
        {
            if (SetField(ref _isChecking, value))
            {
                (CheckNowCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string LastCheckTime
    {
        get => _lastCheckTime;
        private set => SetField(ref _lastCheckTime, value);
    }

    public ICommand CheckNowCommand { get; }

    public TcpPortsViewModel TcpPortsViewModel => _tcpPortsViewModel;

    public ServicesViewModel ServicesViewModel => _servicesViewModel;

    public NetworkViewModel NetworkViewModel => _networkViewModel;

    public SystemResourcesViewModel SystemResourcesViewModel => _systemResourcesViewModel;

    public LogsViewModel LogsViewModel => _logsViewModel;

    private async Task PerformCheck()
    {
        try
        {
            IsChecking = true;
            _messagingCenter.Publish(new SystemCheck(), "SystemCheckStarted");
            
            var check = await _systemCheckService.PerformSystemCheckAsync();
            LastCheck = check;
            await _systemCheckService.PushCheckResultAsync(check);
            
            _messagingCenter.Publish(check, "SystemCheckCompleted");
            LastCheckTime = $"Last check: {DateTime.Now:yyyy/MM/dd HH:mm:ss}";
        }
        finally
        {
            IsChecking = false;
        }
    }

    private void OnSystemCheckCompleted(SystemCheck check)
    {
        LastCheck = check;
        IsChecking = false;
    }

    private void OnSystemCheckStarted()
    {
        IsChecking = true;
    }

    public void Dispose()
    {
        _logsViewModel.Dispose();
    }
}