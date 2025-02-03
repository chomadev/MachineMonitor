using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using SystemChecker.Core.Models;
using SystemChecker.Core.Interfaces;
using SystemChecker.Infrastructure.Services;
using SystemChecker.WPF.Services;

namespace SystemChecker.Tests.Services
{
    public class SystemCheckServiceTests
    {
        private readonly Mock<IServiceChecker> _serviceCheckerMock;
        private readonly Mock<INetworkChecker> _networkCheckerMock;
        private readonly Mock<IDiskChecker> _diskCheckerMock;
        private readonly Mock<IResourceChecker> _resourceCheckerMock;
        private readonly Mock<IConfigurationService> _configServiceMock;
        private readonly Mock<ITcpPortChecker> _portCheckerMock;
        private readonly Mock<IFolderMonitor> _folderMonitorMock;
        private readonly SystemCheckService _systemCheckService;

        public SystemCheckServiceTests()
        {
            _serviceCheckerMock = new Mock<IServiceChecker>();
            _networkCheckerMock = new Mock<INetworkChecker>();
            _diskCheckerMock = new Mock<IDiskChecker>();
            _resourceCheckerMock = new Mock<IResourceChecker>();
            _configServiceMock = new Mock<IConfigurationService>();
            _portCheckerMock = new Mock<ITcpPortChecker>();
            _folderMonitorMock = new Mock<IFolderMonitor>();

            _systemCheckService = new SystemCheckService(
                _serviceCheckerMock.Object,
                _networkCheckerMock.Object,
                _diskCheckerMock.Object,
                _resourceCheckerMock.Object,
                _configServiceMock.Object,
                _portCheckerMock.Object,
                _folderMonitorMock.Object);
        }

        [Fact]
        public async Task PerformSystemCheckAsync_ShouldUseUpdatedServiceList()
        {
            // Arrange
            var monitoredServices = new List<string> { "Service1", "Service2" };
            _configServiceMock.Setup(x => x.GetMonitoredServices())
                .Returns(monitoredServices);

            var expectedServices = new[]
            {
                new ServiceStatus { Name = "Service1", IsRunning = true, Status = "Running" },
                new ServiceStatus { Name = "Service2", IsRunning = true, Status = "Running" }
            };

            _serviceCheckerMock.Setup(x => x.CheckServicesAsync(monitoredServices))
                .ReturnsAsync(expectedServices);

            SetupOtherMocks();

            // Act
            var result = await _systemCheckService.PerformSystemCheckAsync();

            // Assert
            Assert.Equal(expectedServices.Length, result.Services.Length);
            Assert.Equal(expectedServices[0].Name, result.Services[0].Name);
            Assert.Equal(expectedServices[1].Name, result.Services[1].Name);
            
            _serviceCheckerMock.Verify(x => x.CheckServicesAsync(monitoredServices), Times.Once);
        }

        [Fact]
        public async Task PerformSystemCheckAsync_ShouldNotIncludeRemovedServices()
        {
            // Arrange
            var updatedServices = new List<string> { "Service1", "Service2" };
            _configServiceMock.Setup(x => x.GetMonitoredServices())
                .Returns(updatedServices);

            var expectedServices = new[]
            {
                new ServiceStatus { Name = "Service1", IsRunning = true, Status = "Running" },
                new ServiceStatus { Name = "Service2", IsRunning = true, Status = "Running" }
            };

            _serviceCheckerMock.Setup(x => x.CheckServicesAsync(updatedServices))
                .ReturnsAsync(expectedServices);

            SetupOtherMocks();

            // Act
            var result = await _systemCheckService.PerformSystemCheckAsync();

            // Assert
            Assert.Equal(2, result.Services.Length);
            Assert.DoesNotContain(result.Services, s => s.Name == "Service3");
            
            _serviceCheckerMock.Verify(x => x.CheckServicesAsync(updatedServices), Times.Once);
        }

        private void SetupOtherMocks()
        {
            _networkCheckerMock.Setup(x => x.CheckNetworkAsync())
                .ReturnsAsync(new NetworkStatus());
            
            _diskCheckerMock.Setup(x => x.CheckDisksAsync())
                .ReturnsAsync(Array.Empty<DiskStatus>());
            
            _resourceCheckerMock.Setup(x => x.CheckCpuAsync())
                .ReturnsAsync(new CpuStatus());

            _resourceCheckerMock.Setup(x => x.CheckMemoryAsync())
                .ReturnsAsync(new MemoryStatus());

            _portCheckerMock.Setup(x => x.CheckPortsAsync())
                .ReturnsAsync(Array.Empty<TcpPortStatus>());

            _folderMonitorMock.Setup(x => x.GetChanges())
                .Returns(Array.Empty<FolderChangeStatus>());
        }
    }
} 