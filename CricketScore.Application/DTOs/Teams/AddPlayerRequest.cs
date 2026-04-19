namespace CricketScore.Application.DTOs.Teams;

public record AddPlayerRequest(
    string PlayerId,
    bool IsCaptain = false,
    bool IsWicketKeeper = false
);
