using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using SystemChecker.Core.Interfaces;
using SystemChecker.WPF.Services;
using SystemChecker.WPF.Settings;
using SystemChecker.Core.Models;
using SystemChecker.Infrastructure.Services;

namespace SystemChecker.Tests.Services
{
    public class SchedulerServiceTests
    {
        private readonly Mock<ISystemCheckService> _systemCheckServiceMock;
        private readonly Mock<ITrayIconService> _trayIconServiceMock;
        private readonly Mock<IOptions<SchedulerSettings>> _settingsMock;
        private readonly SchedulerService _schedulerService;
        private readonly SchedulerSettings _settings;

        public SchedulerServiceTests()
        {
            _systemCheckServiceMock = new Mock<ISystemCheckService>();
            _trayIconServiceMock = new Mock<ITrayIconService>();
            _settingsMock = new Mock<IOptions<SchedulerSettings>>();

            _settings = new SchedulerSettings
            {
                CheckSchedule = "*/5 * * * *" // A cada 5 minutos
            };

            _settingsMock.Setup(x => x.Value).Returns(_settings);

            _schedulerService = new SchedulerService(
                _systemCheckServiceMock.Object,
                _trayIconServiceMock.Object,
                _settingsMock.Object);
        }

        [Fact]
        public async Task StartAsync_DeveIniciarTimer()
        {
            // Act
            await _schedulerService.StartAsync(CancellationToken.None);

            // Assert
            Assert.Equal(_settings.CheckSchedule, _schedulerService.CurrentSchedule);
        }

        [Fact]
        public async Task StopAsync_DeveInterromperTimer()
        {
            // Arrange
            await _schedulerService.StartAsync(CancellationToken.None);

            // Act
            await _schedulerService.StopAsync(CancellationToken.None);

            // Assert
            // Aguarda para garantir que nenhuma verificação seja executada
            await Task.Delay(100);
            _systemCheckServiceMock.Verify(x => x.PerformSystemCheckAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateSchedule_DeveAtualizarCronExpression()
        {
            // Arrange
            var novoSchedule = "*/1 * * * *";
            var scheduleChangedCalled = false;
            _schedulerService.ScheduleChanged += (sender, schedule) => scheduleChangedCalled = true;

            // Act
            await _schedulerService.UpdateSchedule(novoSchedule);

            // Assert
            Assert.Equal(novoSchedule, _schedulerService.CurrentSchedule);
            Assert.True(scheduleChangedCalled, "O evento ScheduleChanged deveria ter sido disparado");
        }

        [Fact]
        public async Task DoWork_DeveExecutarVerificacaoNoHorarioProgramado()
        {
            // Arrange
            var result = new SystemCheck
            {
                Timestamp = DateTime.Now,
                Services = Array.Empty<ServiceStatus>()
            };

            _systemCheckServiceMock.Setup(x => x.PerformSystemCheckAsync())
                .ReturnsAsync(result);

            // Act
            await _schedulerService.StartAsync(CancellationToken.None);
            
            // Aguarda tempo suficiente para a primeira execução
            await Task.Delay(100);

            // Assert
            _systemCheckServiceMock.Verify(x => x.PushCheckResultAsync(It.IsAny<SystemCheck>()), Times.AtLeastOnce);
            _trayIconServiceMock.Verify(x => x.ShowNotification(
                "Verificação Agendada",
                "A verificação do sistema foi concluída com sucesso."), 
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task DoWork_DeveTratarErrosDuranteVerificacao()
        {
            // Arrange
            _systemCheckServiceMock.Setup(x => x.PerformSystemCheckAsync())
                .ThrowsAsync(new Exception("Erro simulado"));

            // Act
            await _schedulerService.StartAsync(CancellationToken.None);
            
            // Aguarda tempo suficiente para a primeira execução
            await Task.Delay(100);

            // Assert
            _trayIconServiceMock.Verify(x => x.ShowNotification(
                "Erro na Verificação",
                It.Is<string>(msg => msg.Contains("Erro simulado"))),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task RestartWithNewSchedule_DeveReiniciarComNovoAgendamento()
        {
            // Arrange
            var novoSchedule = "*/1 * * * *";

            // Act
            await _schedulerService.RestartWithNewSchedule(novoSchedule);

            // Assert
            Assert.Equal(novoSchedule, _schedulerService.CurrentSchedule);
        }

        [Fact]
        public void Dispose_DeveLiberarRecursos()
        {
            // Act
            _schedulerService.Dispose();

            // Assert
            // Não há como verificar diretamente se o Timer foi disposed
            // mas podemos garantir que não há exceções
        }

        [Theory]
        [InlineData("*/5 * * * *")]
        [InlineData("0 */1 * * *")]
        [InlineData("0 0 * * *")]
        public async Task UpdateSchedule_DeveAceitarDiferentesExpressoesCron(string cronExpression)
        {
            // Act
            await _schedulerService.UpdateSchedule(cronExpression);

            // Assert
            Assert.Equal(cronExpression, _schedulerService.CurrentSchedule);
        }

        [Fact]
        public async Task UpdateSchedule_DeveLancarExcecaoParaExpressaoInvalida()
        {
            // Arrange
            var invalidCron = "invalid cron";

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _schedulerService.UpdateSchedule(invalidCron));
        }
    }
} 