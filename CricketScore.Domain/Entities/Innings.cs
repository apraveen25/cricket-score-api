using Newtonsoft.Json;

namespace CricketScore.Domain.Entities;

public class BatsmanScore
{
    public string PlayerId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public int Runs { get; set; }
    public int Balls { get; set; }
    public int Fours { get; set; }
    public int Sixes { get; set; }
    public bool IsOut { get; set; }
    public string? DismissalDescription { get; set; }
    public bool IsOnStrike { get; set; }
    public bool HasBatted { get; set; }
}

public class BowlerScore
{
    public string PlayerId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public int Overs { get; set; }
    public int BallsInCurrentOver { get; set; }
    public int Maidens { get; set; }
    public int Runs { get; set; }
    public int Wickets { get; set; }
    public int Wides { get; set; }
    public int NoBalls { get; set; }
}

public class FallOfWicket
{
    public int WicketNumber { get; set; }
    public string PlayerId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public int RunsAtFall { get; set; }
    public string Over { get; set; } = string.Empty;
}

public class Innings
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("type")]
    public string Type { get; set; } = "innings";

    public string MatchId { get; set; } = string.Empty;
    public string BattingTeamId { get; set; } = string.Empty;
    public string BowlingTeamId { get; set; } = string.Empty;
    public int InningsNumber { get; set; }
    public int TotalRuns { get; set; }
    public int Wickets { get; set; }
    public int TotalLegalBalls { get; set; }
    public int Extras { get; set; }
    public bool IsCompleted { get; set; }
    public string? CurrentBatsmanId { get; set; }
    public string? NonStrikeBatsmanId { get; set; }
    public string? CurrentBowlerId { get; set; }
    public List<BatsmanScore> BattingScores { get; set; } = [];
    public List<BowlerScore> BowlingScores { get; set; } = [];
    public List<FallOfWicket> FallOfWickets { get; set; } = [];
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
}
