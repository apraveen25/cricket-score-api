using CricketScore.Domain.Enums;

namespace CricketScore.Application.DTOs.Matches;

public record PlayingXIDto(string TeamId, List<string> PlayerIds);

public record CreateMatchRequest(
    string Team1Id,
    string Team2Id,
    MatchFormat Format,
    int OversPerInnings,
    DateTime ScheduledAt,
    string? Venue,
    List<PlayingXIDto> PlayingXIs
);
