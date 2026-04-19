using CricketScore.Domain.Enums;
using Newtonsoft.Json;

namespace CricketScore.Domain.Entities;

public class Player
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("type")]
    public string Type { get; set; } = "player";

    public string Name { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public BattingStyle BattingStyle { get; set; }
    public BowlingStyle BowlingStyle { get; set; }
    public PlayerRole Role { get; set; }
    public int? JerseyNumber { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
