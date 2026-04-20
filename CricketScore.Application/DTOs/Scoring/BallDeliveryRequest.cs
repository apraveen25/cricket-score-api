using CricketScore.Domain.Enums;

namespace CricketScore.Application.DTOs.Scoring;

public record BallDeliveryRequest(
    string BatsmanId,
    string BowlerId,
    int RunsScored,
    ExtraType ExtraType,
    int ExtraRuns,
    bool IsWicket,
    WicketType WicketType,
    string? FielderId,
    string? DismissedBatsmanId,
    string? NextBatsmanId,
    string? NextBowlerId,
    double? ShotAngle = null,
    double? ShotDistance = null
);
