using SystemChecker.Infrastructure.Services;
using Xunit;

namespace SystemChecker.Tests.Infrastructure.Services;

public class DiskCheckerTests
{
    private readonly DiskChecker _diskChecker;

    public DiskCheckerTests()
    {
        _diskChecker = new DiskChecker();
    }

    [Fact]
    public async Task CheckDisksAsync_ShouldReturnDiskStatus()
    {
        // Act
        var result = await _diskChecker.CheckDisksAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        foreach (var disk in result)
        {
            Assert.NotNull(disk.Name);
            Assert.True(disk.TotalSpace > 0);
            Assert.True(disk.UsagePercentage >= 0 && disk.UsagePercentage <= 100);
        }
    }
} 