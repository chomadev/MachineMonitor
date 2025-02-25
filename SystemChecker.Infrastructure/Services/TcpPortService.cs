using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.NetworkInformation;
using SystemChecker.Core.Interfaces;
using SystemChecker.Core.Models;

namespace SystemChecker.Infrastructure.Services
{
    public class TcpPortService : ITcpPortService
    {
        private readonly ILogger<TcpPortService> _logger;
        private readonly IConfigurationService _configService;
        private MachineConfiguration _currentConfig;

        public TcpPortService(
            ILogger<TcpPortService> logger,
            IConfigurationService configService)
        {
            _logger = logger;
            _configService = configService;
            _currentConfig = new MachineConfiguration();
            
            _configService.ConfigurationChanged += async (_, _) => 
            {
                _currentConfig = await _configService.LoadConfigurationAsync();
                _logger.LogInformation("TCP Ports configuration updated: {Ports}", 
                    string.Join(", ", _currentConfig.TcpPorts));
            };

            // Carrega configuração inicial
            Task.Run(async () =>
            {
                try
                {
                    _currentConfig = await _configService.LoadConfigurationAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading initial configuration");
                }
            });
        }

        public IEnumerable<int> GetConfiguredPorts()
        {
            return _currentConfig.TcpPorts;
        }

        public async Task<IEnumerable<TcpPortStatus>> CheckPortsAsync(IEnumerable<int> ports)
        {
            var results = new List<TcpPortStatus>();
            foreach (var port in ports)
            {
                results.Add(await CheckPortAsync(port));
            }
            return results;
        }

        public async Task<TcpPortStatus> CheckPortAsync(int port)
        {
            var portInfo = new TcpPortStatus { Port = port };

            try
            {
                var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                var tcpConnections = ipGlobalProperties.GetActiveTcpConnections()
                    .Where(c => c.LocalEndPoint.Port == port);

                var tcpListeners = ipGlobalProperties.GetActiveTcpListeners()
                    .Where(l => l.Port == port);

                if (tcpConnections.Any() || tcpListeners.Any())
                {
                    portInfo.IsOpen = true;
                    try
                    {
                        using var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "netstat",
                                Arguments = "-ano",
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                CreateNoWindow = true
                            }
                        };

                        process.Start();
                        string output = await process.StandardOutput.ReadToEndAsync();
                        await process.WaitForExitAsync();

                        var lines = output.Split('\n');
                        foreach (var line in lines)
                        {
                            if (line.Contains($":{port} "))
                            {
                                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                if (parts.Length >= 5 && int.TryParse(parts[4], out int pid))
                                {
                                    using var processInfo = Process.GetProcessById(pid);
                                    portInfo.Service = processInfo.ProcessName;
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not get process information via netstat for port {Port}", port);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking port {Port}", port);
                throw;
            }

            return portInfo;
        }
    }
}