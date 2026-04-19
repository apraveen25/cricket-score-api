using CricketScore.Domain.Enums;

namespace CricketScore.Application.DTOs.Players;

public record UpdatePlayerRequest(
    string? Name = null,
    PlayerRole? Role = null,
    BattingStyle? BattingStyle = null,
    BowlingStyle? BowlingStyle = null,
    DateOnly? DateOfBirth = null,
    string? Nationality = null,
    int? JerseyNumber = null
);
