using CricketScore.Domain.Enums;

namespace CricketScore.Application.DTOs.Teams;

public record UpdateTeamPlayerRequest(
    string? Name = null,
    PlayerRole? Role = null,
    bool? IsCaptain = null,
    bool? IsWicketKeeper = null
);
