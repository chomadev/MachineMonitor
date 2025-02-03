using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SystemChecker.Core.Interfaces;
using SystemChecker.Core.Settings;

namespace SystemChecker.Infrastructure.Services
{
    public class SchedulerService : ISchedulerService
    {
        private readonly ILogger<SchedulerService> _logger;
        private readonly SchedulerSettings _settings;

        public string CurrentSchedule => _settings.CheckSchedule;
        public event EventHandler<string>? ScheduleChanged;

        public SchedulerService(
            IOptions<SchedulerSettings> settings,
            ILogger<SchedulerService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task UpdateScheduleAsync(string cronExpression)
        {
            _logger.LogInformation("Atualizando agendamento para: {Schedule}", cronExpression);
            
            try
            {
                // Atualiza as configurações
                _settings.CheckSchedule = cronExpression;
                
                // Notifica a mudança
                ScheduleChanged?.Invoke(this, cronExpression);
                
                _logger.LogInformation("Agendamento atualizado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar agendamento");
                throw;
            }
        }
    }
} 