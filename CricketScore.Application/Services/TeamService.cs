using AutoMapper;
using CricketScore.Application.DTOs.Teams;
using CricketScore.Application.Exceptions;
using CricketScore.Domain.Entities;
using CricketScore.Domain.Interfaces.Repositories;

namespace CricketScore.Application.Services;

public class TeamService(ITeamRepository teamRepository, IMapper mapper)
{
    public async Task<TeamResponse> CreateTeamAsync(CreateTeamRequest request, string userId)
    {
        var team = new Team
        {
            Name = request.Name,
            ShortName = request.ShortName,
            CreatedBy = userId
        };

        var created = await teamRepository.CreateAsync(team);
        return mapper.Map<TeamResponse>(created);
    }

    public async Task<TeamResponse> GetTeamAsync(string id)
    {
        var team = await teamRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Team {id} not found.");

        return mapper.Map<TeamResponse>(team);
    }

    public async Task<TeamResponse> AddPlayerAsync(string teamId, AddPlayerRequest request, string userId)
    {
        var team = await teamRepository.GetByIdAsync(teamId)
            ?? throw new KeyNotFoundException($"Team {teamId} not found.");

        if (team.CreatedBy != userId)
            throw new UnauthorizedAccessException("Only the team creator can add players.");

        if (team.Players.Count >= 15)
            throw new InvalidOperationException("Team cannot have more than 15 players.");

        if (team.Players.Any(p => p.PlayerId == request.PlayerId))
            throw new ConflictException($"Player '{request.Name}' is already in this team.");

        var teamPlayer = new TeamPlayer
        {
            PlayerId = request.PlayerId,
            Name = request.Name,
            Role = request.Role,
            IsCaptain = request.IsCaptain,
            IsWicketKeeper = request.IsWicketKeeper
        };

        team.Players.Add(teamPlayer);
        var updated = await teamRepository.UpdateAsync(team);
        return mapper.Map<TeamResponse>(updated);
    }

    public async Task<IEnumerable<TeamResponse>> GetMyTeamsAsync(string userId)
    {
        var teams = await teamRepository.GetByCreatorAsync(userId);
        return mapper.Map<IEnumerable<TeamResponse>>(teams);
    }
}
