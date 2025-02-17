using Microsoft.Extensions.Logging;
using Moq;
using SystemChecker.Core.Interfaces;
using SystemChecker.Core.Models;
using SystemChecker.WPF.Services;
using Xunit;

namespace SystemChecker.Tests.Services;

public class SchedulerExecutionServiceTests
{
    private readonly Mock<ISystemCheckService> _systemCheckServiceMock;
    private readonly Mock<ITrayIconService> _trayIconServiceMock;
    private readonly Mock<ISchedulerService> _schedulerServiceMock;
    private readonly Mock<ILogger<SchedulerExecutionService>> _loggerMock;
    private readonly SchedulerExecutionService _service;

    public SchedulerExecutionServiceTests()
    {
        _systemCheckServiceMock = new Mock<ISystemCheckService>();
        _trayIconServiceMock = new Mock<ITrayIconService>();
        _schedulerServiceMock = new Mock<ISchedulerService>();
        _loggerMock = new Mock<ILogger<SchedulerExecutionService>>();

        _schedulerServiceMock.Setup(x => x.CurrentSchedule)
            .Returns("*/15 * * * *");

        _service = new SchedulerExecutionService(
            _systemCheckServiceMock.Object,
            _trayIconServiceMock.Object,
            _schedulerServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task StartAsync_ShouldInitializeTimer()
    {
        // Act
        await _service.StartAsync(CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteSystemCheck_ShouldNotifyOnSuccess()
    {
        // Arrange
        var checkResult = new SystemCheck { Timestamp = DateTime.Now };
        _systemCheckServiceMock.Setup(x => x.PerformSystemCheckAsync())
            .ReturnsAsync(checkResult);

        var checkCompletedCalled = false;
        _service.SystemCheckCompleted += (s, e) => checkCompletedCalled = true;

        // Act
        await _service.StartAsync(CancellationToken.None);
        await Task.Delay(100); // Give time for the timer to trigger

        // Assert
        _trayIconServiceMock.Verify(x => x.ShowNotification(
            "Scheduled Check",
            It.Is<string>(msg => msg.Contains("successfully"))),
            Times.AtLeastOnce);
        Assert.True(checkCompletedCalled);
    }

    [Fact]
    public async Task ExecuteSystemCheck_ShouldHandleErrors()
    {
        // Arrange
        _systemCheckServiceMock.Setup(x => x.PerformSystemCheckAsync())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        await _service.StartAsync(CancellationToken.None);
        await Task.Delay(100); // Give time for the timer to trigger

        // Assert
        _trayIconServiceMock.Verify(x => x.ShowNotification(
            "Check Error",
            It.Is<string>(msg => msg.Contains("error"))),
            Times.AtLeastOnce);
    }
} 