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
        private readonly List<int> _configuredPorts;

        public TcpPortService(
            ILogger<TcpPortService> logger,
            IConfigurationService configService)
        {
            _logger = logger;
            _configService = configService;
            _configuredPorts = new List<int>();
            LoadConfiguredPorts();
        }

        private void LoadConfiguredPorts()
        {
            var ports = _configService.GetMonitoredPorts();
            _configuredPorts.Clear();
            _configuredPorts.AddRange(ports);
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
                        _logger.LogWarning(ex, "Não foi possível obter informações do processo via netstat para a porta {Port}", port);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar a porta {Port}", port);
                throw;
            }

            return portInfo;
        }

        public IEnumerable<int> GetConfiguredPorts() => _configuredPorts.ToList();

        public async Task UpdateConfiguredPortsAsync(IEnumerable<int> ports)
        {
            await _configService.UpdateMonitoredPortsAsync(ports);
            _configuredPorts.Clear();
            _configuredPorts.AddRange(ports);
        }
    }
}