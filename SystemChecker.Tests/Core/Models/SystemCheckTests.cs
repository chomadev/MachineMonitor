using SystemChecker.Core.Models;
using Xunit;

namespace SystemChecker.Tests.Core.Models;

public class SystemCheckTests
{
    [Fact]
    public void Constructor_ShouldInitializeAllProperties()
    {
        // Act
        var systemCheck = new SystemCheck();

        // Assert
        Assert.NotNull(systemCheck.Services);
        Assert.NotNull(systemCheck.Network);
        Assert.NotNull(systemCheck.Disks);
        Assert.NotNull(systemCheck.Cpu);
        Assert.NotNull(systemCheck.Memory);
        Assert.NotNull(systemCheck.Ports);
        Assert.NotNull(systemCheck.FolderChanges);
        
        Assert.Empty(systemCheck.Services);
        Assert.Empty(systemCheck.Network.ActiveInterfaces);
        Assert.Empty(systemCheck.Disks);
        Assert.Empty(systemCheck.Ports);
        Assert.Empty(systemCheck.FolderChanges);
    }
} 