using CricketScore.Domain.Enums;

namespace CricketScore.Application.DTOs.Players;

public record PlayerResponse(
    string Id,
    string Name,
    PlayerRole Role,
    BattingStyle BattingStyle,
    BowlingStyle BowlingStyle,
    DateOnly? DateOfBirth,
    string? Nationality,
    int? JerseyNumber,
    string CreatedBy,
    DateTime CreatedAt
);
