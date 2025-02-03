//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Cronos;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Options;
//using SystemChecker.Core.Interfaces;
//using SystemChecker.WPF.Settings;

//namespace SystemChecker.WPF.Services;

//public class SchedulerService : IHostedService, IDisposable
//{
//    private readonly ISystemCheckService _systemCheckService;
//    private readonly ITrayIconService _trayIconService;
//    private readonly SchedulerSettings _settings;
//    private Timer _timer;
//    private CronExpression _cronExpression;
//    private DateTime? _nextRun;

//    public SchedulerService(
//        ISystemCheckService systemCheckService,
//        ITrayIconService trayIconService,
//        IOptions<SchedulerSettings> settings)
//    {
//        _systemCheckService = systemCheckService;
//        _trayIconService = trayIconService;
//        _settings = settings.Value;
//        _cronExpression = CronExpression.Parse(_settings.CheckSchedule);
//    }

//    public Task StartAsync(CancellationToken cancellationToken)
//    {
//        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
//        return Task.CompletedTask;
//    }

//    private async void DoWork(object state)
//    {
//        var utcNow = DateTime.UtcNow;
        
//        if (_nextRun == null)
//        {
//            _nextRun = _cronExpression.GetNextOccurrence(utcNow);
//            return;
//        }

//        if (utcNow >= _nextRun)
//        {
//            try
//            {
//                var result = await _systemCheckService.PerformSystemCheckAsync();
//                await _systemCheckService.PushCheckResultAsync(result);
//                _trayIconService.ShowNotification(
//                    "Verificação Agendada", 
//                    "A verificação do sistema foi concluída com sucesso.");
//            }
//            catch (Exception ex)
//            {
//                _trayIconService.ShowNotification(
//                    "Erro na Verificação", 
//                    $"Ocorreu um erro durante a verificação: {ex.Message}");
//            }

//            _nextRun = _cronExpression.GetNextOccurrence(utcNow);
//        }
//    }

//    public Task StopAsync(CancellationToken cancellationToken)
//    {
//        _timer?.Change(Timeout.Infinite, 0);
//        return Task.CompletedTask;
//    }

//    public void Dispose()
//    {
//        _timer?.Dispose();
//    }

//    public async Task RestartWithNewSchedule(string newSchedule)
//    {
//        await StopAsync(CancellationToken.None);
        
//        _settings.CheckSchedule = newSchedule;
//        _cronExpression = CronExpression.Parse(newSchedule);
//        _nextRun = null;
        
//        await StartAsync(CancellationToken.None);
//    }

//    public string CurrentSchedule => _settings.CheckSchedule;

//    public event EventHandler<string> ScheduleChanged;

//    public async Task UpdateSchedule(string newSchedule)
//    {
//        await StopAsync(CancellationToken.None);
        
//        _settings.CheckSchedule = newSchedule;
//        _cronExpression = CronExpression.Parse(newSchedule);
//        _nextRun = null;
        
//        await StartAsync(CancellationToken.None);
        
//        ScheduleChanged?.Invoke(this, newSchedule);
//    }
//} 