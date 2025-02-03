using Cronos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SystemChecker.Core.Interfaces;
using SystemChecker.Core.Models;

namespace SystemChecker.WPF.Services
{
    public class SchedulerExecutionService : IHostedService, IDisposable
    {
        private readonly ISystemCheckService _systemCheckService;
        private readonly ITrayIconService _trayIconService;
        private readonly ISchedulerService _schedulerService;
        private readonly ILogger<SchedulerExecutionService> _logger;
        private Timer? _timer;
        private CronExpression _cronExpression;
        private DateTime? _nextRun;

        public event EventHandler<SystemCheck>? SystemCheckCompleted;
        public event EventHandler? SystemCheckStarted;

        public SchedulerExecutionService(
            ISystemCheckService systemCheckService,
            ITrayIconService trayIconService,
            ISchedulerService schedulerService,
            ILogger<SchedulerExecutionService> logger)
        {
            _systemCheckService = systemCheckService;
            _trayIconService = trayIconService;
            _schedulerService = schedulerService;
            _logger = logger;
            _cronExpression = CronExpression.Parse(_schedulerService.CurrentSchedule);

            // Inscreve no evento de mudança de agendamento
            _schedulerService.ScheduleChanged += OnScheduleChanged;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando SchedulerExecutionService com expressão CRON: {Schedule}",
                _schedulerService.CurrentSchedule);

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Parando SchedulerExecutionService");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private void OnScheduleChanged(object? sender, string newSchedule)
        {
            _cronExpression = CronExpression.Parse(newSchedule);
            _nextRun = null;
            _logger.LogInformation("Agendamento atualizado para: {Schedule}", newSchedule);
        }

        private async void DoWork(object? state)
        {
            try
            {
                var utcNow = DateTime.UtcNow;

                if (_nextRun == null)
                {
                    _nextRun = _cronExpression.GetNextOccurrence(utcNow);
                    return;
                }

                if (utcNow >= _nextRun)
                {
                    await ExecuteSystemCheck();
                    _nextRun = _cronExpression.GetNextOccurrence(utcNow);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no processamento do agendador");
            }
        }

        private async Task ExecuteSystemCheck()
        {
            try
            {
                _logger.LogInformation("Iniciando verificação do sistema");
                
                // Notifica início da verificação
                SystemCheckStarted?.Invoke(this, EventArgs.Empty);
                
                var result = await _systemCheckService.PerformSystemCheckAsync();
                await _systemCheckService.PushCheckResultAsync(result);
                
                SystemCheckCompleted?.Invoke(this, result);
                
                _trayIconService.ShowNotification(
                    "Verificação Agendada",
                    "A verificação do sistema foi concluída com sucesso.");
                
                _logger.LogInformation("Verificação do sistema concluída com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante a verificação do sistema");
                _trayIconService.ShowNotification(
                    "Erro na Verificação",
                    $"Ocorreu um erro durante a verificação: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _schedulerService.ScheduleChanged -= OnScheduleChanged;
        }
    }
}