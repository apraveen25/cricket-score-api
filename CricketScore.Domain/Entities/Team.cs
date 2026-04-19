using CricketScore.Domain.Enums;
using Newtonsoft.Json;

namespace CricketScore.Domain.Entities;

public class TeamPlayer
{
    public string PlayerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public PlayerRole Role { get; set; }
    public bool IsCaptain { get; set; }
    public bool IsWicketKeeper { get; set; }
}

public class Team
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("type")]
    public string Type { get; set; } = "team";

    public string Name { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public List<TeamPlayer> Players { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
