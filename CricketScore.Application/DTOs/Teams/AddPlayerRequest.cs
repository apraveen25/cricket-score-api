using CricketScore.Domain.Enums;

namespace CricketScore.Application.DTOs.Teams;

public record AddPlayerRequest(
    string Name,
    PlayerRole Role,
    BattingStyle BattingStyle,
    BowlingStyle BowlingStyle,
    bool IsCaptain = false,
    bool IsWicketKeeper = false,
    DateOnly? DateOfBirth = null,
    string? Nationality = null,
    int? JerseyNumber = null
);
