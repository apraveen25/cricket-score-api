using AutoMapper;
using CricketScore.Application.DTOs.Matches;
using CricketScore.Application.DTOs.Teams;
using CricketScore.Domain.Entities;

namespace CricketScore.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Team, TeamResponse>()
            .ConstructUsing(src => new TeamResponse(
                src.Id, src.Name, src.CreatedBy,
                src.Players.Select(p => new TeamPlayerDto(p.PlayerId, p.Name, p.Role, p.IsCaptain, p.IsWicketKeeper)).ToList(),
                src.CreatedAt));

        CreateMap<Match, MatchResponse>()
            .ConstructUsing(src => new MatchResponse(
                src.Id, src.Team1Id, src.Team2Id, src.Format, src.OversPerInnings,
                src.Status,
                src.Toss != null ? new TossResultDto(src.Toss.WinnerTeamId, src.Toss.Decision) : null,
                src.BattingFirstTeamId, src.CurrentInningsId, src.Venue,
                src.ScheduledAt, src.CreatedAt));
    }
}
