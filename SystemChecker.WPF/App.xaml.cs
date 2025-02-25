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
using Microsoft.Extensions.Configuration;
using SystemChecker.Core.Services;
using System.Net.Http;

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
                // Core Services
                services.AddSingleton<IMessagingCenter, MessagingCenter>();
                services.AddSingleton<IConfigurationService, ConfigurationService>();

                // Infrastructure Services
                services.AddSingleton<IServiceChecker, ServiceChecker>();
                services.AddSingleton<INetworkChecker, NetworkChecker>();
                services.AddSingleton<IDiskChecker, DiskChecker>();
                services.AddSingleton<IResourceChecker, ResourceChecker>();
                services.AddSingleton<ITcpPortService, TcpPortService>();
                services.AddSingleton<IFolderMonitor, FolderMonitor>();

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

                // System Check Service
                services.AddHttpClient();
                services.AddSingleton<ISystemCheckService, SystemCheckService>(sp =>
                {
                    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient();
                    var apiSettings = sp.GetRequiredService<IConfiguration>()
                        .GetSection("ApiSettings")
                        .Get<ApiSettings>();
                    
                    httpClient.BaseAddress = new Uri(apiSettings.BaseUrl);
                    httpClient.DefaultRequestHeaders.Add("ApiKey", apiSettings.MachineKey);
                    httpClient.Timeout = TimeSpan.FromSeconds(30);
                    
                    return new SystemCheckService(
                        sp.GetRequiredService<ILogger<SystemCheckService>>(),
                        sp.GetRequiredService<IServiceChecker>(),
                        sp.GetRequiredService<INetworkChecker>(),
                        sp.GetRequiredService<IDiskChecker>(),
                        sp.GetRequiredService<IResourceChecker>(),
                        sp.GetRequiredService<IConfigurationService>(),
                        sp.GetRequiredService<ITcpPortService>(),
                        sp.GetRequiredService<IFolderMonitor>(),
                        httpClient,
                        sp.GetRequiredService<IConfiguration>()
                    );
                });

                // UI Services
                services.AddSingleton<ITrayIconService, TrayIconService>();
                services.AddSingleton<ISchedulerService, SchedulerService>();

                // UI
                services.AddSingleton<ConfigurationViewModel>();
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<MainWindow>();
                services.AddSingleton<TcpPortsViewModel>();
                services.AddSingleton<ServicesViewModel>();
                services.AddSingleton<SystemResourcesViewModel>();
                services.AddSingleton<LogsViewModel>();
                services.AddSingleton<NetworkViewModel>();
                services.AddSingleton<FolderMonitorViewModel>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        var trayService = _host.Services.GetRequiredService<ITrayIconService>();
        trayService.Initialize();

        // Load initial settings
        var configViewModel = _host.Services.GetRequiredService<ConfigurationViewModel>();
        await configViewModel.LoadInitialConfiguration();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        // Perform initial check after window is shown
        var systemCheckService = _host.Services.GetRequiredService<ISystemCheckService>();
        await systemCheckService.PerformSystemCheckAsync();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        base.OnExit(e);
    }
}