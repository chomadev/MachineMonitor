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
using Microsoft.Extensions.Logging;
using SystemChecker.Infrastructure.Settings;

namespace SystemChecker.WPF;

public partial class App : Application
{
    private readonly IHost _host;
    public static bool IsShuttingDown { get; set; }

    public App()
    {
        IsShuttingDown = false;

        // Create logger provider first to avoid dependency cycle
        var loggerProvider = new UiLoggerProvider();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSystemChecker(context.Configuration);

                // Scheduler Config
                services.Configure<SchedulerSettings>(
                    context.Configuration.GetSection(nameof(SchedulerSettings)));

                // API Config
                services.Configure<ApiSettings>(context.Configuration.GetSection("ApiSettings"));

                // Logging
                services.AddSingleton(loggerProvider);
                services.AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddProvider(loggerProvider);
                    builder.SetMinimumLevel(LogLevel.Information);
                });

                // Services
                services.AddSingleton<ITrayIconService, TrayIconService>();
                services.AddSingleton<ISchedulerService, SchedulerService>();
                //services.AddSingleton<SchedulerExecutionService>();
                //services.AddHostedService(sp => sp.GetRequiredService<SchedulerExecutionService>());
                services.AddSingleton<IConfigurationService, ConfigurationService>();
                services.AddSingleton<ITcpPortService, TcpPortService>();
                services.AddHttpClient<ISystemCheckService, SystemCheckService>();

                // UI
                services.AddSingleton<ConfigurationViewModel>();
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<MainWindow>();
                services.AddSingleton<TcpPortsViewModel>();
                services.AddSingleton<LogsViewModel>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        var trayService = _host.Services.GetRequiredService<ITrayIconService>();
        trayService.Initialize();

        // Load initial settings
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