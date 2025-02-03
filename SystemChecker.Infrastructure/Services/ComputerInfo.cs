using System.Runtime.InteropServices;

namespace SystemChecker.Infrastructure.Services;

public class ComputerInfo
{
    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);

    public long TotalPhysicalMemory
    {
        get
        {
            GetPhysicallyInstalledSystemMemory(out long memoryInKb);
            return memoryInKb * 1024; // Convertendo para bytes
        }
    }

    public long AvailablePhysicalMemory
    {
        get
        {
            var gcMemoryInfo = GC.GetGCMemoryInfo();
            return gcMemoryInfo.TotalAvailableMemoryBytes;
        }
    }
} 