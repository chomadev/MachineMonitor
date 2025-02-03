using Moq;
using SystemChecker.Core.Interfaces;
using SystemChecker.Core.Models;
using SystemChecker.Infrastructure.Services;
using Xunit;

namespace SystemChecker.Tests.Infrastructure.Services;

public class SystemCheckServiceTests
{
    private readonly Mock<IServiceChecker> _serviceCheckerMock;
    private readonly Mock<INetworkChecker> _networkCheckerMock;
    private readonly Mock<IDiskChecker> _diskCheckerMock;
    private readonly Mock<IResourceChecker> _resourceCheckerMock;
    private readonly Mock<ITcpPortChecker> _portCheckerMock;
    private readonly Mock<IFolderMonitor> _folderMonitorMock;
    private readonly Mock<IConfigurationService> _configurationServiceMock;
    private readonly SystemCheckService _systemCheckService;

    public SystemCheckServiceTests()
    {
        _serviceCheckerMock = new Mock<IServiceChecker>();
        _networkCheckerMock = new Mock<INetworkChecker>();
        _diskCheckerMock = new Mock<IDiskChecker>();
        _resourceCheckerMock = new Mock<IResourceChecker>();
        _portCheckerMock = new Mock<ITcpPortChecker>();
        _folderMonitorMock = new Mock<IFolderMonitor>();
        _configurationServiceMock = new Mock<IConfigurationService>();

        _systemCheckService = new SystemCheckService(
            _serviceCheckerMock.Object,
            _networkCheckerMock.Object,
            _diskCheckerMock.Object,
            _resourceCheckerMock.Object,
            _configurationServiceMock.Object,
            _portCheckerMock.Object,
            _folderMonitorMock.Object
        );

        SetupMocks();
    }

    private void SetupMocks()
    {
        _serviceCheckerMock.Setup(x => x.CheckServicesAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new ServiceStatus[] { new() { Name = "Test" } });

        _networkCheckerMock.Setup(x => x.CheckNetworkAsync())
            .ReturnsAsync(new NetworkStatus());

        _diskCheckerMock.Setup(x => x.CheckDisksAsync())
            .ReturnsAsync(new DiskStatus[] { new() { DriveLetter = "C:" } });

        _resourceCheckerMock.Setup(x => x.CheckCpuAsync())
            .ReturnsAsync(new CpuStatus());

        _resourceCheckerMock.Setup(x => x.CheckMemoryAsync())
            .ReturnsAsync(new MemoryStatus());

        _portCheckerMock.Setup(x => x.CheckPortsAsync())
            .ReturnsAsync(new TcpPortStatus[] { new() { Port = 80 } });

        _folderMonitorMock.Setup(x => x.GetChanges())
            .Returns(new FolderChangeStatus[] { new() { Path = "test" } });
    }

    [Fact]
    public async Task PerformSystemCheckAsync_ShouldReturnCompleteSystemCheck()
    {
        // Act
        var result = await _systemCheckService.PerformSystemCheckAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.NotEqual(default, result.Timestamp);
        
        Assert.Single(result.Services);
        Assert.NotNull(result.Network);
        Assert.Single(result.Disks);
        Assert.NotNull(result.Cpu);
        Assert.NotNull(result.Memory);
        Assert.Single(result.Ports);
        Assert.Single(result.FolderChanges);

        // Verify all checks were called
        _serviceCheckerMock.Verify(x => x.CheckServicesAsync(It.IsAny<List<string>>()), Times.Once);
        _networkCheckerMock.Verify(x => x.CheckNetworkAsync(), Times.Once);
        _diskCheckerMock.Verify(x => x.CheckDisksAsync(), Times.Once);
        _resourceCheckerMock.Verify(x => x.CheckCpuAsync(), Times.Once);
        _resourceCheckerMock.Verify(x => x.CheckMemoryAsync(), Times.Once);
        _portCheckerMock.Verify(x => x.CheckPortsAsync(), Times.Once);
        _folderMonitorMock.Verify(x => x.GetChanges(), Times.Once);
    }
} 