using CricketScore.Domain.Enums;
using Newtonsoft.Json;

namespace CricketScore.Domain.Entities;

public class User
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("type")]
    public string Type { get; set; } = "user";

    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Player;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
