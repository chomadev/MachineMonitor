using System.Collections.ObjectModel;
using SystemChecker.Core.Models;
using SystemChecker.Core.Interfaces;

namespace SystemChecker.WPF.ViewModels;

public class ServicesViewModel : ViewModelBase
{
    private readonly IMessagingCenter _messagingCenter;
    private ObservableCollection<ServiceStatus> _services;

    public ServicesViewModel(IMessagingCenter messagingCenter)
    {
        _messagingCenter = messagingCenter;
        _services = new ObservableCollection<ServiceStatus>();

        // Subscribe to system check updates
        _messagingCenter.Subscribe<SystemCheck>(this, "SystemCheckCompleted", OnSystemCheckCompleted);
    }

    public ObservableCollection<ServiceStatus> Services
    {
        get => _services;
        private set => SetField(ref _services, value);
    }

    private void OnSystemCheckCompleted(SystemCheck systemCheck)
    {
        if (systemCheck.Services != null)
        {
            Services.Clear();
            foreach (var service in systemCheck.Services)
            {
                Services.Add(service);
            }
        }
    }
} 