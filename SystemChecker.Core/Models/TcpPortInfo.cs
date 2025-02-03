namespace SystemChecker.Core.Models
{
    public class TcpPortInfo
    {
        public int Port { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public bool IsInUse { get; set; }
        public string? ProcessName { get; set; }
        public int? ProcessId { get; set; }
    }
} 