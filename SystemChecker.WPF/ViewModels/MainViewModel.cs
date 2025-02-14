using System.Windows;
using System.Windows.Input;
using SystemChecker.Core.Interfaces;
using SystemChecker.Core.Models;
using SystemChecker.WPF.Commands;
using SystemChecker.WPF.Services;

namespace SystemChecker.WPF.ViewModels;

public class MainViewModel : ViewModelBase, IDisposable
{
    private readonly ISystemCheckService _systemCheckService;
    private readonly IMessagingCenter _messagingCenter;
    private SystemCheck _lastCheck;
    private bool _isChecking;
    private ConfigurationViewModel _configViewModel;
    private string _lastCheckTime;
    private SystemCheck? _lastCheckResult;
    private readonly TcpPortsViewModel _tcpPortsViewModel;
    private readonly LogsViewModel _logsViewModel;

    public MainViewModel(
        ISystemCheckService systemCheckService,
        IMessagingCenter messagingCenter,
        ConfigurationViewModel configViewModel,
        TcpPortsViewModel tcpPortsViewModel,
        LogsViewModel logsViewModel)
    {
        _systemCheckService = systemCheckService;
        _messagingCenter = messagingCenter;
        _configViewModel = configViewModel;
        _lastCheckTime = "None check performed";
        _tcpPortsViewModel = tcpPortsViewModel;
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

    public SystemCheck LastCheck
    {
        get => _lastCheck;
        private set
        {
            if (SetField(ref _lastCheck, value))
            {
                OnPropertyChanged(nameof(LastCheck.Services));
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

    public SystemCheck? LastCheckResult
    {
        get => _lastCheckResult;
        private set => SetField(ref _lastCheckResult, value);
    }

    public ICommand CheckNowCommand { get; }

    public TcpPortsViewModel TcpPortsViewModel => _tcpPortsViewModel;

    public LogsViewModel LogsViewModel => _logsViewModel;

    private async Task PerformCheck()
    {
        try
        {
            IsChecking = true;
            LastCheck = await _systemCheckService.PerformSystemCheckAsync();
            await _systemCheckService.PushCheckResultAsync(LastCheck);
            LastCheckTime = $"Last check: {DateTime.Now:yyyy/MM/dd HH:mm:ss}";
        }
        finally
        {
            IsChecking = false;
        }
    }

    private void OnSystemCheckCompleted(SystemCheck check)
    {
        LastCheckResult = check;
        LastCheckTime = check.Timestamp.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");
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