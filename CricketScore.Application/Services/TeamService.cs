using AutoMapper;
using CricketScore.Application.DTOs.Teams;
using CricketScore.Domain.Entities;
using CricketScore.Domain.Interfaces.Repositories;

namespace CricketScore.Application.Services;

public class TeamService(ITeamRepository teamRepository, IPlayerRepository playerRepository, IMapper mapper)
{
    public async Task<TeamResponse> CreateTeamAsync(CreateTeamRequest request, string userId)
    {
        var team = new Team
        {
            Name = request.Name,
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
            throw new InvalidOperationException("Player is already in this team.");

        var player = await playerRepository.GetByIdAsync(request.PlayerId)
            ?? throw new KeyNotFoundException($"Player {request.PlayerId} not found.");

        var teamPlayer = new TeamPlayer
        {
            PlayerId = player.Id,
            Name = player.Name,
            Role = player.Role,
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
