using System.Collections.ObjectModel;
using System.ServiceProcess;
using System.Windows.Input;
using SystemChecker.Core.Interfaces;
using SystemChecker.WPF.Commands;

namespace SystemChecker.WPF.ViewModels;

public class ConfigurationViewModel : ViewModelBase
{
    private readonly IConfigurationService _configService;
    private string _cronExpression;
    private string _validationMessage;
    private string _selectedService;
    private string _newServiceName;
    private ObservableCollection<string> _monitoredServices;

    public ConfigurationViewModel(IConfigurationService configService)
    {
        _configService = configService;

        // Load the initial settings
        LoadInitialConfiguration();

        SaveCommand = new AsyncRelayCommand(SaveConfiguration, CanSaveConfiguration);
        ValidateCommand = new RelayCommand(ValidateCronExpression);
        AddServiceCommand = new RelayCommand(AddService, CanAddService);
        RemoveServiceCommand = new RelayCommand(RemoveService, CanRemoveService);
    }

    public async Task LoadInitialConfiguration()
    {
        try
        {
            // Load the CRON schedule
            _cronExpression = _configService.GetCurrentSchedule();
            OnPropertyChanged(nameof(CronExpression));

            // Load the monitored services list
            _monitoredServices = new ObservableCollection<string>(_configService.GetMonitoredServices());
            OnPropertyChanged(nameof(MonitoredServices));

            // Validate the initial CRON expression
            ValidateCronExpression();

            // Notify that the settings were loaded
            ValidationMessage = "Settings loaded successfully!";
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error loading settings: {ex.Message}";
        }
    }

    public ObservableCollection<string> MonitoredServices
    {
        get => _monitoredServices;
        set => SetField(ref _monitoredServices, value);
    }

    public string SelectedService
    {
        get => _selectedService;
        set
        {
            if (SetField(ref _selectedService, value))
            {
                (RemoveServiceCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string NewServiceName
    {
        get => _newServiceName;
        set
        {
            if (SetField(ref _newServiceName, value))
            {
                (AddServiceCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string CronExpression
    {
        get => _cronExpression;
        set
        {
            if (SetField(ref _cronExpression, value))
            {
                ValidateCronExpression();
                (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string ValidationMessage
    {
        get => _validationMessage;
        private set => SetField(ref _validationMessage, value);
    }

    public ICommand SaveCommand { get; }
    public ICommand ValidateCommand { get; }
    public ICommand AddServiceCommand { get; }
    public ICommand RemoveServiceCommand { get; }

    private bool CanSaveConfiguration()
    {
        try
        {
            Cronos.CronExpression.Parse(_cronExpression);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool CanAddService()
    {
        if (string.IsNullOrWhiteSpace(NewServiceName) ||
            MonitoredServices.Contains(NewServiceName))
            return false;

        try
        {
            var service = new ServiceController(NewServiceName);
            return true;
        }
        catch
        {
            ValidationMessage = $"Service '{NewServiceName}' not found in system";
            return false;
        }
    }

    private void AddService()
    {
        if (CanAddService())
        {
            MonitoredServices.Add(NewServiceName);
            NewServiceName = string.Empty;
            OnPropertyChanged(nameof(NewServiceName));
            ValidationMessage = "Service added successfully";
        }
    }

    private bool CanRemoveService()
    {
        return !string.IsNullOrWhiteSpace(SelectedService);
    }

    private void RemoveService()
    {
        if (CanRemoveService())
        {
            MonitoredServices.Remove(SelectedService);
            ValidationMessage = "Service removed successfully";
        }
    }

    private async Task SaveConfiguration()
    {
        try
        {
            await _configService.UpdateConfiguration(
                _cronExpression,
                MonitoredServices.ToList());

            ValidationMessage = "Settings saved successfully!";
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error saving: {ex.Message}";
        }
    }

    private void ValidateCronExpression()
    {
        try
        {
            var cron = Cronos.CronExpression.Parse(_cronExpression);
            var nextRun = cron.GetNextOccurrence(DateTime.UtcNow);
            ValidationMessage = $"Valid! Next execution: {nextRun?.ToLocalTime():dd/MM/yyyy HH:mm:ss}";
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Invalid CRON expression: {ex.Message}";
        }
    }
}