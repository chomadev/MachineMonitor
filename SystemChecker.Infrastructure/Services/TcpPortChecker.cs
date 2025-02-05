using Microsoft.Extensions.Options;
using System.Net.Sockets;
using SystemChecker.Core.Models;
using SystemChecker.Infrastructure.Settings;

namespace SystemChecker.Infrastructure.Services;

public interface ITcpPortChecker
{
    Task<TcpPortStatus[]> CheckPortsAsync();
}

public class TcpPortChecker : ITcpPortChecker
{
    private readonly TcpPortSettings _settings;

    public TcpPortChecker(IOptions<TcpPortSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<TcpPortStatus[]> CheckPortsAsync()
    {
        var results = new List<TcpPortStatus>();

        foreach (var portConfig in _settings.Ports)
        {
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync("localhost", portConfig.Port);

                if (await Task.WhenAny(connectTask, Task.Delay(1000)) == connectTask)
                {
                    results.Add(new TcpPortStatus
                    {
                        Port = portConfig.Port,
                        IsOpen = client.Connected,
                        Service = portConfig.ServiceName
                    });
                }
                else
                {
                    results.Add(new TcpPortStatus
                    {
                        Port = portConfig.Port,
                        IsOpen = false,
                        Service = portConfig.ServiceName
                    });
                }
            }
            catch
            {
                results.Add(new TcpPortStatus
                {
                    Port = portConfig.Port,
                    IsOpen = false,
                    Service = portConfig.ServiceName
                });
            }
        }

        return results.ToArray();
    }
}