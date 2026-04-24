using CricketScore.Domain.Enums;

namespace CricketScore.Application.DTOs.Teams;

public record TeamPlayerDto(
    int SerialNo,
    string PlayerId,
    string Name,
    PlayerRole Role,
    bool IsCaptain,
    bool IsWicketKeeper
);

public record TeamResponse(
    string Id,
    string Name,
    string ShortName,
    string CreatedBy,
    List<TeamPlayerDto> Players,
    DateTime CreatedAt
);
