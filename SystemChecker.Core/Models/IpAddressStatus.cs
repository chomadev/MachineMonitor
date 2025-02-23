namespace SystemChecker.Core.Models;

public class IpAddressStatus
{
    public string Address { get; set; } = string.Empty;
    public bool IsReachable { get; set; }
    public long ResponseTime { get; set; }  // em millisegundos
} 