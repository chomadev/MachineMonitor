using SystemChecker.API.Models;

namespace SystemChecker.API.Services;

public class InMemorySystemCheckStore : ISystemCheckStore
{
    private SystemCheckData? _latestCheck;
    private readonly object _lock = new();

    public Task SaveCheckAsync(SystemCheckData check)
    {
        lock (_lock)
        {
            _latestCheck = check;
        }
        return Task.CompletedTask;
    }

    public Task<SystemCheckData?> GetLatestCheckAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_latestCheck);
        }
    }
} 