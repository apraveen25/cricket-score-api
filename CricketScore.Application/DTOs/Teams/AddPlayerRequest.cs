using CricketScore.Domain.Enums;

namespace CricketScore.Application.DTOs.Teams;

public record AddPlayerRequest(
    string PlayerId,
    string Name,
    PlayerRole Role,
    bool IsCaptain = false,
    bool IsWicketKeeper = false
);
