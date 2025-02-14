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
        private IEnumerable<int> _cachedPorts;

        public TcpPortService(
            ILogger<TcpPortService> logger,
            IConfigurationService configService)
        {
            _logger = logger;
            _configService = configService;
            _cachedPorts = LoadConfiguredPorts();
            
            _configService.ConfigurationChanged += (_, _) => 
            {
                _cachedPorts = LoadConfiguredPorts();
                _logger.LogInformation("TCP Ports configuration updated: {Ports}", 
                    string.Join(", ", _cachedPorts));
            };
        }

        private IEnumerable<int> LoadConfiguredPorts()
        {
            var portsString = _configService.GetMonitoredPorts();
            return string.IsNullOrEmpty(portsString)
                ? Enumerable.Empty<int>()
                : portsString.Split(',')
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => int.Parse(p.Trim()));
        }

        public IEnumerable<int> GetConfiguredPorts()
        {
            return _cachedPorts;
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

        public async Task UpdateConfiguredPortsAsync(IEnumerable<int> ports)
        {
            await _configService.UpdateMonitoredPortsAsync(ports);
        }
    }
}