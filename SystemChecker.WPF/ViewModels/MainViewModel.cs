using System.Threading.Tasks;
using System.Windows.Input;
using SystemChecker.Core.Interfaces;
using SystemChecker.Core.Models;
using SystemChecker.WPF.Commands;
using System.Windows;
using System;
using SystemChecker.WPF.Services;

namespace SystemChecker.WPF.ViewModels;

public class MainViewModel : ViewModelBase, IDisposable
{
    private readonly ISystemCheckService _systemCheckService;
    private readonly SchedulerExecutionService _schedulerExecutionService;
    private SystemCheck _lastCheck;
    private bool _isChecking;
    private ConfigurationViewModel _configViewModel;
    private string _lastCheckTime;
    private SystemCheck? _lastCheckResult;

    public MainViewModel(
        ISystemCheckService systemCheckService,
        SchedulerExecutionService schedulerExecutionService,
        ConfigurationViewModel configViewModel)
    {
        _systemCheckService = systemCheckService;
        _schedulerExecutionService = schedulerExecutionService;
        _configViewModel = configViewModel;
        _lastCheckTime = "Nenhuma verificação realizada";
        
        CheckNowCommand = new AsyncRelayCommand(PerformCheck, () => !IsChecking);
        
        // Inscreve nos eventos
        _schedulerExecutionService.SystemCheckCompleted += OnSystemCheckCompleted;
        _schedulerExecutionService.SystemCheckStarted += OnSystemCheckStarted;
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

    private async Task PerformCheck()
    {
        try
        {
            IsChecking = true;
            LastCheck = await _systemCheckService.PerformSystemCheckAsync();
            await _systemCheckService.PushCheckResultAsync(LastCheck);
            LastCheckTime = $"Última verificação: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
        }
        finally
        {
            IsChecking = false;
        }
    }

    private void OnSystemCheckStarted(object? sender, EventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            IsChecking = true;
        });
    }

    private void OnSystemCheckCompleted(object? sender, SystemCheck result)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            IsChecking = false;
            LastCheck = result;
            LastCheckResult = result;
            LastCheckTime = $"Última verificação: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
            
            OnPropertyChanged(nameof(LastCheck));
            OnPropertyChanged(nameof(LastCheck.Services));
            OnPropertyChanged(nameof(LastCheck.Cpu));
            OnPropertyChanged(nameof(LastCheck.Memory));
            OnPropertyChanged(nameof(LastCheck.Network));
            OnPropertyChanged(nameof(LastCheck.Disks));
        });
    }

    public void Dispose()
    {
        _schedulerExecutionService.SystemCheckCompleted -= OnSystemCheckCompleted;
        _schedulerExecutionService.SystemCheckStarted -= OnSystemCheckStarted;
    }
} 