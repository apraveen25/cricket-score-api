using CricketScore.Domain.Enums;

namespace CricketScore.Application.DTOs.Matches;

public record StartMatchRequest(
    string TossWinnerTeamId,
    TossDecision TossDecision
);
