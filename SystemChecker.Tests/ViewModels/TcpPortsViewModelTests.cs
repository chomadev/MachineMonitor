using Moq;
using SystemChecker.Core.Interfaces;
using SystemChecker.Core.Models;
using SystemChecker.WPF.ViewModels;
using Xunit;

namespace SystemChecker.Tests.ViewModels;

public class TcpPortsViewModelTests : TestBase
{
    private readonly Mock<ITcpPortService> _tcpPortServiceMock;
    private readonly TcpPortsViewModel _viewModel;

    public TcpPortsViewModelTests()
    {
        _tcpPortServiceMock = new Mock<ITcpPortService>();
        _tcpPortServiceMock.Setup(x => x.GetConfiguredPorts())
            .Returns(new[] { 80, 443 });

        _viewModel = new TcpPortsViewModel(_tcpPortServiceMock.Object);
    }

    [Fact]
    public void Constructor_ShouldLoadConfiguredPorts()
    {
        // Assert
        Assert.Equal(2, _viewModel.TcpPorts.Count);
        Assert.Contains(_viewModel.TcpPorts, p => p.Port == 80);
        Assert.Contains(_viewModel.TcpPorts, p => p.Port == 443);
    }

    [Fact]
    public async Task CheckPorts_ShouldUpdatePortStatus()
    {
        // Arrange
        _tcpPortServiceMock.Setup(x => x.CheckPortsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new[]
            {
                new TcpPortInfo { Port = 80, IsInUse = true, ProcessName = "TestProcess" },
                new TcpPortInfo { Port = 443, IsInUse = false }
            });

        // Act
        await _viewModel.CheckPortsCommand.ExecuteAsync(null);

        // Assert
        Assert.True(_viewModel.TcpPorts.First(p => p.Port == 80).IsInUse);
        Assert.False(_viewModel.TcpPorts.First(p => p.Port == 443).IsInUse);
    }
} 