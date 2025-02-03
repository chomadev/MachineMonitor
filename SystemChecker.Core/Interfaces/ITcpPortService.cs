using SystemChecker.Core.Models;

namespace SystemChecker.Core.Interfaces
{
    public interface ITcpPortService
    {
        Task<IEnumerable<TcpPortStatus>> CheckPortsAsync(IEnumerable<int> ports);
        Task<TcpPortStatus> CheckPortAsync(int port);
        IEnumerable<int> GetConfiguredPorts();
        Task UpdateConfiguredPortsAsync(IEnumerable<int> ports);
    }
} 