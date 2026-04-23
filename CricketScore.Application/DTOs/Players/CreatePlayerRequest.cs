using CricketScore.Domain.Enums;

namespace CricketScore.Application.DTOs.Players;

public record CreatePlayerRequest(
    string Name,
    PlayerRole PlayerRole,
    BattingStyle BattingStyle,
    BowlingStyle BowlingStyle,
    DateOnly? DateOfBirth = null,
    string? Nationality = null,
    int? JerseyNumber = null
);
