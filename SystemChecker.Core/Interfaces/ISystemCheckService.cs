using SystemChecker.Core.Models;

namespace SystemChecker.Core.Interfaces;

public interface ISystemCheckService
{
    Task<SystemCheck> PerformSystemCheckAsync();
    Task<bool> PushCheckResultAsync(SystemCheck check);
}