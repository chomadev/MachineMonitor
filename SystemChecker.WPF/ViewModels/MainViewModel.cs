using System.Windows.Input;
using SystemChecker.Core.Interfaces;
using SystemChecker.Core.Models;
using SystemChecker.WPF.Commands;

namespace SystemChecker.WPF.ViewModels;

public class MainViewModel : ViewModelBase, IDisposable
{
    private readonly ISystemCheckService _systemCheckService;
    private readonly IMessagingCenter _messagingCenter;
    private SystemCheck? _lastCheck;
    private bool _isChecking;
    private ConfigurationViewModel _configViewModel;
    private string _lastCheckTime;
    private readonly TcpPortsViewModel _tcpPortsViewModel;
    private readonly LogsViewModel _logsViewModel;
    private readonly ServicesViewModel _servicesViewModel;
    private readonly SystemResourcesViewModel _systemResourcesViewModel;

    public MainViewModel(
        ISystemCheckService systemCheckService,
        IMessagingCenter messagingCenter,
        ConfigurationViewModel configViewModel,
        TcpPortsViewModel tcpPortsViewModel,
        ServicesViewModel servicesViewModel,
        SystemResourcesViewModel systemResourcesViewModel,
        LogsViewModel logsViewModel)
    {
        _systemCheckService = systemCheckService;
        _messagingCenter = messagingCenter;
        _configViewModel = configViewModel;
        _lastCheckTime = "None check performed";
        _tcpPortsViewModel = tcpPortsViewModel;
        _servicesViewModel = servicesViewModel;
        _systemResourcesViewModel = systemResourcesViewModel;
        _logsViewModel = logsViewModel;

        CheckNowCommand = new AsyncRelayCommand(PerformCheck, () => !IsChecking);

        // Subscribe to messages instead of direct events
        _messagingCenter.Subscribe<SystemCheck>(this, "SystemCheckCompleted", OnSystemCheckCompleted);
        _messagingCenter.Subscribe<object>(this, "SystemCheckStarted", _ => OnSystemCheckStarted());
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

    public SystemResourcesViewModel SystemResourcesViewModel => _systemResourcesViewModel;

    public LogsViewModel LogsViewModel => _logsViewModel;

    private async Task PerformCheck()
    {
        try
        {
            IsChecking = true;
            var check = await _systemCheckService.PerformSystemCheckAsync();
            LastCheck = check;
            await _systemCheckService.PushCheckResultAsync(check);
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