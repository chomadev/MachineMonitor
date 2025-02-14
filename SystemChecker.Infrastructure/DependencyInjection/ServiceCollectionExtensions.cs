using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SystemChecker.Core.Interfaces;
using SystemChecker.Core.Services;
using SystemChecker.Infrastructure.Services;
using SystemChecker.Infrastructure.Settings;

namespace SystemChecker.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSystemChecker(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Settings
        services.Configure<ServiceSettings>(
            configuration.GetSection(nameof(ServiceSettings)));
        
        services.Configure<TcpPortSettings>(
            configuration.GetSection(nameof(TcpPortSettings)));
        
        services.Configure<FolderMonitorSettings>(
            configuration.GetSection(nameof(FolderMonitorSettings)));

        // Services
        services.AddSingleton<ISystemCheckService, SystemCheckService>();
        services.AddSingleton<IServiceChecker, ServiceChecker>();
        services.AddSingleton<INetworkChecker, NetworkChecker>();
        services.AddSingleton<IDiskChecker, DiskChecker>();
        services.AddSingleton<IResourceChecker, ResourceChecker>();
        services.AddSingleton<IFolderMonitor, FolderMonitor>();
        services.AddSingleton<IMessagingCenter, MessagingCenter>();
        services.AddHostedService<SchedulerExecutionService>();

        return services;
    }
} 