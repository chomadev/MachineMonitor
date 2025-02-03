using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using SystemChecker.Core.Interfaces;
using SystemChecker.Core.Settings;
using SystemChecker.Infrastructure.DependencyInjection;
using SystemChecker.Infrastructure.Services;
using SystemChecker.WPF.Services;
using SystemChecker.WPF.ViewModels;
using SystemChecker.WPF.Views;

namespace SystemChecker.WPF;

public partial class App : Application
{
    private readonly IHost _host;
    public static bool IsShuttingDown { get; set; }

    public App()
    {
        IsShuttingDown = false;
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSystemChecker(context.Configuration);

                // Configurações
                services.Configure<SchedulerSettings>(
                    context.Configuration.GetSection(nameof(SchedulerSettings)));

                // Serviços
                services.AddSingleton<ITrayIconService, TrayIconService>();
                services.AddSingleton<ISchedulerService, SchedulerService>();
                services.AddSingleton<SchedulerExecutionService>();
                services.AddHostedService(sp => sp.GetRequiredService<SchedulerExecutionService>());
                services.AddSingleton<IConfigurationService, ConfigurationService>();

                // UI
                services.AddSingleton<ConfigurationViewModel>();
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        var trayService = _host.Services.GetRequiredService<ITrayIconService>();
        trayService.Initialize();

        // Carrega as configurações iniciais
        // var configService = _host.Services.GetRequiredService<IConfigurationService>();
        var configViewModel = _host.Services.GetRequiredService<ConfigurationViewModel>();
        await configViewModel.LoadInitialConfiguration();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        base.OnExit(e);
    }
}