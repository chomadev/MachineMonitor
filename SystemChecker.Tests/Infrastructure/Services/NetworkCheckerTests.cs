using SystemChecker.Infrastructure.Services;
using Xunit;

namespace SystemChecker.Tests.Infrastructure.Services;

public class NetworkCheckerTests
{
    private readonly NetworkChecker _networkChecker;

    public NetworkCheckerTests()
    {
        _networkChecker = new NetworkChecker();
    }

    [Fact]
    public async Task CheckNetworkAsync_ShouldReturnNetworkStatus()
    {
        // Act
        var result = await _networkChecker.CheckNetworkAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ActiveInterfaces);
    }
} 