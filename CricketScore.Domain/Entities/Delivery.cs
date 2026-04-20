using CricketScore.Domain.Enums;
using Newtonsoft.Json;

namespace CricketScore.Domain.Entities;

public class Delivery
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("type")]
    public string Type { get; set; } = "delivery";

    public string InningsId { get; set; } = string.Empty;
    public string MatchId { get; set; } = string.Empty;
    public int OverNumber { get; set; }
    public int BallInOver { get; set; }
    public string BatsmanId { get; set; } = string.Empty;
    public string BowlerId { get; set; } = string.Empty;
    public int RunsScored { get; set; }
    public ExtraType ExtraType { get; set; } = ExtraType.None;
    public int ExtraRuns { get; set; }
    public bool IsWicket { get; set; }
    public WicketType WicketType { get; set; } = WicketType.None;
    public string? FielderId { get; set; }
    public string? DismissedBatsmanId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public double? ShotAngle { get; set; }
    public double? ShotDistance { get; set; }
    public InningsStateSnapshot? PreDeliverySnapshot { get; set; }
}
