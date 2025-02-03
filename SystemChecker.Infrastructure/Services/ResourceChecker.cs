using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SystemChecker.Core.Models;

namespace SystemChecker.Infrastructure.Services;

public interface IResourceChecker
{
    Task<CpuStatus> CheckCpuAsync();
    Task<MemoryStatus> CheckMemoryAsync();
}

public class ResourceChecker : IResourceChecker
{
    private readonly PerformanceCounter _cpuCounter;
    private readonly ComputerInfo _computerInfo;

    public ResourceChecker()
    {
        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        _computerInfo = new ComputerInfo();
    }

    public async Task<CpuStatus> CheckCpuAsync()
    {
        _cpuCounter.NextValue(); // Primeira leitura sempre retorna 0
        await Task.Delay(1000); // Aguarda 1 segundo para leitura precisa

        return new CpuStatus
        {
            UsagePercentage = Math.Round(_cpuCounter.NextValue(), 2),
            ProcessCount = Process.GetProcesses().Length
        };
    }

    public Task<MemoryStatus> CheckMemoryAsync()
    {
        var totalPhysicalMemory = _computerInfo.TotalPhysicalMemory;
        var availablePhysicalMemory = _computerInfo.AvailablePhysicalMemory;
        var usagePercentage = Math.Round(((double)(totalPhysicalMemory - availablePhysicalMemory) / totalPhysicalMemory) * 100, 2);

        return Task.FromResult(new MemoryStatus
        {
            TotalPhysicalMemory = totalPhysicalMemory,
            AvailablePhysicalMemory = availablePhysicalMemory,
            UsagePercentage = usagePercentage
        });
    }
}