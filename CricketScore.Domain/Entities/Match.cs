using CricketScore.Domain.Enums;
using Newtonsoft.Json;

namespace CricketScore.Domain.Entities;

public class TossResult
{
    public string WinnerTeamId { get; set; } = string.Empty;
    public TossDecision Decision { get; set; }
}

public class PlayingXI
{
    public string TeamId { get; set; } = string.Empty;
    public List<string> PlayerIds { get; set; } = [];
}

public class Match
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("type")]
    public string Type { get; set; } = "match";

    public string Team1Id { get; set; } = string.Empty;
    public string Team2Id { get; set; } = string.Empty;
    public MatchFormat Format { get; set; }
    public int OversPerInnings { get; set; }
    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;
    public TossResult? Toss { get; set; }
    public string? BattingFirstTeamId { get; set; }
    public List<PlayingXI> PlayingXIs { get; set; } = [];
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Venue { get; set; }
    public string? CurrentInningsId { get; set; }
}
