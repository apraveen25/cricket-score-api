using CricketScore.Domain.Enums;

namespace CricketScore.Application.DTOs.Matches;

public record TossResultDto(string WinnerTeamId, TossDecision Decision);

public record MatchResponse(
    string Id,
    string Team1Id,
    string Team2Id,
    MatchFormat Format,
    int OversPerInnings,
    MatchStatus Status,
    TossResultDto? Toss,
    string? BattingFirstTeamId,
    string? CurrentInningsId,
    string? Venue,
    DateTime ScheduledAt,
    DateTime CreatedAt
);
