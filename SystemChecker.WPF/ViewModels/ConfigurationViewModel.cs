using System.Collections.ObjectModel;
using System.ServiceProcess;
using System.Windows.Input;
using SystemChecker.Core.Interfaces;
using SystemChecker.WPF.Commands;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace SystemChecker.WPF.ViewModels;

public class ConfigurationViewModel : ViewModelBase
{
    private readonly IConfigurationService _configService;
    private readonly ILogger<ConfigurationViewModel> _logger;
    private string _cronExpression;
    private string _validationMessage;
    private string _selectedService;
    private string _newServiceName;
    private ObservableCollection<string> _monitoredServices;
    private string _tcpPorts;

    public ConfigurationViewModel(
        IConfigurationService configService,
        ILogger<ConfigurationViewModel> logger)
    {
        _configService = configService;
        _logger = logger;

        // Load the initial settings
        LoadInitialConfiguration();

        SaveCommand = new AsyncRelayCommand(SaveConfiguration, CanSaveConfiguration);
        ValidateCronExpressionCommand = new RelayCommand(ValidateCronExpression);
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

            // Load the monitored tcp ports list
            _tcpPorts = _configService.GetMonitoredPorts();
            OnPropertyChanged(nameof(TcpPorts));

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

    public string TcpPorts
    {
        get => _tcpPorts;
        set
        {
            if (SetField(ref _tcpPorts, value))
            {
                ValidateTcpPorts();
                (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand SaveCommand { get; }
    public ICommand ValidateCronExpressionCommand { get; }
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
            // Update schedule
            await _configService.UpdateScheduleAsync(_cronExpression);

            // Update services
            await _configService.UpdateMonitoredServicesAsync(_monitoredServices.ToList());

            // Update TCP ports
            await _configService.UpdateMonitoredPortsAsync(
                _tcpPorts.Split(',')
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => int.Parse(p.Trim())));

            ValidationMessage = "Settings saved successfully!";
            _logger.LogInformation("All configuration settings updated successfully");
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error saving settings: {ex.Message}";
            _logger.LogError(ex, "Error saving configuration settings");
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

    private void ValidateTcpPorts()
    {
        if (!string.IsNullOrEmpty(_tcpPorts))
        {
            var ports = _tcpPorts.Split(",");
            var invalidPorts = new List<string>();
            foreach (var port in ports) {
                if (!int.TryParse(port, out var portNumber))
                {
                    invalidPorts.Add(port);
                }
            }

            if (invalidPorts.Any())
            {
                ValidationMessage = $"Invalid TCP Ports: `{string.Join(",", invalidPorts)}`";
            }
            else
            {
                ValidationMessage = "TCP Ports set successfully";
            }
        }
    }
}