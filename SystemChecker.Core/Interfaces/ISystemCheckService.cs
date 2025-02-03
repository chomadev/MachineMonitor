using SystemChecker.Core.Models;
using System.Threading.Tasks;

namespace SystemChecker.Core.Interfaces;

public interface ISystemCheckService
{
    Task<SystemCheck> PerformSystemCheckAsync();
    Task<bool> PushCheckResultAsync(SystemCheck check);
} 