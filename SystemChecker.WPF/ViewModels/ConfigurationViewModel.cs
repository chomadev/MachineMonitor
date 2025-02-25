using System.Collections.ObjectModel;
using System.ServiceProcess;
using System.Windows.Input;
using SystemChecker.Core.Interfaces;
using SystemChecker.WPF.Commands;
using System.Linq;
using Microsoft.Extensions.Logging;
using SystemChecker.Core.Models;

namespace SystemChecker.WPF.ViewModels;

public class ConfigurationViewModel : ViewModelBase
{
    private readonly IConfigurationService _configService;
    private readonly ILogger<ConfigurationViewModel> _logger;
    private MachineConfiguration _currentConfig;
    private string _validationMessage = string.Empty;
    private string _cronExpression = string.Empty;
    private string _tcpPorts = string.Empty;
    private string _monitoredIpAddresses = string.Empty;
    private string _newServiceName = string.Empty;
    private string _newFolderPath = string.Empty;
    private string? _selectedService;
    private FolderMonitorConfig? _selectedFolder;
    private ObservableCollection<string> _monitoredServices = new ObservableCollection<string>();
    private ObservableCollection<FolderMonitorConfig> _monitoredFolders = new ObservableCollection<FolderMonitorConfig>();

    public ConfigurationViewModel(
        IConfigurationService configService,
        ILogger<ConfigurationViewModel> logger)
    {
        _configService = configService;
        _logger = logger;
        _currentConfig = new MachineConfiguration();

        // Commands
        SaveCommand = new AsyncRelayCommand(SaveConfigurationAsync);
        AddServiceCommand = new RelayCommand(AddService, CanAddService);
        RemoveServiceCommand = new RelayCommand(RemoveService, CanRemoveService);
        AddFolderCommand = new RelayCommand(AddFolder, CanAddFolder);
        BrowseFolderCommand = new RelayCommand(BrowseFolder);
    }

    public async Task LoadInitialConfiguration()
    {
        try
        {
            _currentConfig = await _configService.LoadConfigurationAsync();
            
            // Update UI properties from _currentConfig
            CronExpression = _currentConfig.CheckSchedule;
            TcpPorts = string.Join(",", _currentConfig.TcpPorts);
            MonitoredIpAddresses = string.Join(",", _currentConfig.IpAddressesToMonitor);
            MonitoredServices.Clear();
            foreach (var service in _currentConfig.ServicesToMonitor)
            {
                MonitoredServices.Add(service);
            }
            MonitoredFolders.Clear();
            foreach (var folder in _currentConfig.MonitoredFolders)
            {
                MonitoredFolders.Add(folder);
            }

            ValidationMessage = "Configuration loaded successfully";
            _logger.LogInformation("Configuration loaded successfully");
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error loading configuration: {ex.Message}";
            _logger.LogError(ex, "Error loading configuration");
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

    public string MonitoredIpAddresses
    {
        get => _monitoredIpAddresses;
        set => SetField(ref _monitoredIpAddresses, value);
    }

    public ObservableCollection<FolderMonitorConfig> MonitoredFolders
    {
        get => _monitoredFolders;
        set => SetField(ref _monitoredFolders, value);
    }

    public string NewFolderPath
    {
        get => _newFolderPath;
        set => SetField(ref _newFolderPath, value);
    }

    public FolderMonitorConfig? SelectedFolder
    {
        get => _selectedFolder;
        set => SetField(ref _selectedFolder, value);
    }

    public ICommand SaveCommand { get; }
    public ICommand ValidateCronExpressionCommand { get; }
    public ICommand AddServiceCommand { get; }
    public ICommand RemoveServiceCommand { get; }
    public ICommand AddFolderCommand { get; }
    public ICommand RemoveFolderCommand { get; }
    public ICommand BrowseFolderCommand { get; }

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

    private void BrowseFolder()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog();
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            NewFolderPath = dialog.SelectedPath;
        }
    }

    private bool CanAddFolder() => !string.IsNullOrWhiteSpace(NewFolderPath);

    private void AddFolder()
    {
        var folder = new FolderMonitorConfig
        {
            Path = NewFolderPath,
            MonitorLastModified = true  // default values
        };
        MonitoredFolders.Add(folder);
        NewFolderPath = string.Empty;
    }

    private bool CanRemoveFolder() => SelectedFolder != null;

    private void RemoveFolder()
    {
        if (SelectedFolder != null)
        {
            MonitoredFolders.Remove(SelectedFolder);
        }
    }

    private async Task SaveConfigurationAsync()
    {
        try
        {
            // Update _currentConfig from UI properties
            _currentConfig.CheckSchedule = CronExpression;
            _currentConfig.TcpPorts = TcpPorts.Split(',')
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => int.Parse(p.Trim()))
                .ToList();
            _currentConfig.IpAddressesToMonitor = MonitoredIpAddresses.Split(',')
                .Where(ip => !string.IsNullOrWhiteSpace(ip))
                .Select(ip => ip.Trim())
                .ToList();
            _currentConfig.ServicesToMonitor = MonitoredServices.ToList();
            _currentConfig.MonitoredFolders = MonitoredFolders.ToList();

            await _configService.SaveConfigurationAsync(_currentConfig);

            ValidationMessage = "Configuration saved successfully";
            _logger.LogInformation("Configuration saved successfully");
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error saving configuration: {ex.Message}";
            _logger.LogError(ex, "Error saving configuration");
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