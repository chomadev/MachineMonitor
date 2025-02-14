using System.Text.Json.Serialization;

namespace SystemChecker.API.Models;

public class Machine
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    [JsonIgnore]
    public ApiKey? ApiKey { get; set; }
} 