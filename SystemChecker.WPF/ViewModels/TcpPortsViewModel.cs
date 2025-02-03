using System.Collections.ObjectModel;
using System.Windows.Input;
using SystemChecker.Core.Interfaces;
using SystemChecker.Core.Models;
using SystemChecker.WPF.Commands;

namespace SystemChecker.WPF.ViewModels
{
    public class TcpPortsViewModel : ViewModelBase
    {
        private readonly ITcpPortService _tcpPortService;
        private ObservableCollection<TcpPortInfo> _tcpPorts;
        private bool _isChecking;

        public TcpPortsViewModel(ITcpPortService tcpPortService)
        {
            _tcpPortService = tcpPortService;
            _tcpPorts = new ObservableCollection<TcpPortInfo>();

            SavePortsCommand = new AsyncRelayCommand(SavePorts, () => !IsChecking);
            CheckPortsCommand = new AsyncRelayCommand(CheckPorts, () => !IsChecking);

            LoadConfiguredPorts();
        }

        public ObservableCollection<TcpPortInfo> TcpPorts
        {
            get => _tcpPorts;
            set => SetField(ref _tcpPorts, value);
        }

        public bool IsChecking
        {
            get => _isChecking;
            set
            {
                if (SetField(ref _isChecking, value))
                {
                    (SavePortsCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                    (CheckPortsCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand SavePortsCommand { get; }
        public ICommand CheckPortsCommand { get; }

        private void LoadConfiguredPorts()
        {
            var ports = _tcpPortService.GetConfiguredPorts();
            TcpPorts.Clear();
            foreach (var port in ports)
            {
                TcpPorts.Add(new TcpPortInfo { Port = port });
            }
        }

        private async Task SavePorts()
        {
            try
            {
                IsChecking = true;
                var ports = TcpPorts.Select(p => p.Port);
                await _tcpPortService.UpdateConfiguredPortsAsync(ports);
                await CheckPorts();
            }
            finally
            {
                IsChecking = false;
            }
        }

        private async Task CheckPorts()
        {
            try
            {
                IsChecking = true;
                var ports = TcpPorts.Select(p => p.Port);
                var results = await _tcpPortService.CheckPortsAsync(ports);

                foreach (var result in results)
                {
                    TcpPorts.First(x => x.Port == result.Port).IsInUse = true;
                }
            }
            finally
            {
                IsChecking = false;
            }
        }
    }
}