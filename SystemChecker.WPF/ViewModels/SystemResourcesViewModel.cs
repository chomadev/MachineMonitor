using SystemChecker.Core.Models;
using SystemChecker.Core.Interfaces;
using System.Collections.ObjectModel;

namespace SystemChecker.WPF.ViewModels;

public class SystemResourcesViewModel : ViewModelBase
{
    private readonly IMessagingCenter _messagingCenter;
    private CpuStatus _cpu;
    private MemoryStatus _memory;
    private ObservableCollection<DiskStatus> _disks;

    public SystemResourcesViewModel(IMessagingCenter messagingCenter)
    {
        _messagingCenter = messagingCenter;
        _cpu = new CpuStatus();
        _memory = new MemoryStatus();
        _disks = new ObservableCollection<DiskStatus>();

        // Subscribe to system check updates
        _messagingCenter.Subscribe<SystemCheck>(this, "SystemCheckCompleted", OnSystemCheckCompleted);
    }

    public CpuStatus Cpu
    {
        get => _cpu;
        private set => SetField(ref _cpu, value);
    }

    public MemoryStatus Memory
    {
        get => _memory;
        private set => SetField(ref _memory, value);
    }

    public ObservableCollection<DiskStatus> Disks
    {
        get => _disks;
        private set => SetField(ref _disks, value);
    }

    private void OnSystemCheckCompleted(SystemCheck systemCheck)
    {
        Cpu = systemCheck.Cpu;
        Memory = systemCheck.Memory;

        Disks.Clear();
        foreach (var disk in systemCheck.Disks)
        {
            Disks.Add(disk);
        }
    }
} 