namespace SystemChecker.API.Models;

public class ApiKey
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsed { get; set; }
    
    public int MachineId { get; set; }
    public Machine Machine { get; set; } = null!;
} 