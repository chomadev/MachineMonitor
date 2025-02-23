using SystemChecker.Core.Models;

namespace SystemChecker.Infrastructure.Services;

public interface IDiskChecker
{
    Task<DiskStatus[]> CheckDisksAsync();
}

public class DiskChecker : IDiskChecker
{
    public Task<DiskStatus[]> CheckDisksAsync()
    {
        var drives = DriveInfo.GetDrives()
            .Where(d => d.IsReady)
            .Select(d => new DiskStatus
            {
                Name = d.Name,
                TotalSpace = d.TotalSize,
                FreeSpace = d.AvailableFreeSpace,
                UsagePercentage = (double)(d.TotalSize - d.AvailableFreeSpace) / d.TotalSize * 100
            })
            .ToArray();

        return Task.FromResult(drives);
    }
} 