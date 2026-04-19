using CricketScore.Domain.Enums;

namespace CricketScore.Application.DTOs.Teams;

public record AddPlayerRequest(
    string Name,
    PlayerRole Role,
    bool IsCaptain = false,
    bool IsWicketKeeper = false
);
