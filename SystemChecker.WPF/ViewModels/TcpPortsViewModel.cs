using System.Collections.ObjectModel;
using SystemChecker.Core.Models;
using SystemChecker.Core.Interfaces;
using SystemChecker.Core.Services;

namespace SystemChecker.WPF.ViewModels;

public class TcpPortsViewModel : ViewModelBase
{
    private readonly IMessagingCenter _messagingCenter;
    private ObservableCollection<TcpPortStatus> _tcpPortStatuses;

    public TcpPortsViewModel(
        IMessagingCenter messagingCenter)
    {
        _messagingCenter = messagingCenter;
        _tcpPortStatuses = new ObservableCollection<TcpPortStatus>();

        // Subscribe to system check updates
        _messagingCenter.Subscribe<SystemCheck>(this, "SystemCheckCompleted", OnSystemCheckCompleted);
    }

    public ObservableCollection<TcpPortStatus> TcpPortStatuses
    {
        get => _tcpPortStatuses;
        private set => SetField(ref _tcpPortStatuses, value);
    }

    private void OnSystemCheckCompleted(SystemCheck systemCheck)
    {
        if (systemCheck.Ports != null)
        {
            UpdatePortStatuses(systemCheck.Ports);
        }
    }

    public void UpdatePortStatuses(IEnumerable<TcpPortStatus> portStatuses)
    {
        TcpPortStatuses.Clear();
        foreach (var status in portStatuses)
        {
            TcpPortStatuses.Add(status);
        }
    }
}