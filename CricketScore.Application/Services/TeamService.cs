using AutoMapper;
using CricketScore.Application.DTOs.Teams;
using CricketScore.Domain.Entities;
using CricketScore.Domain.Interfaces.Repositories;
using CricketScore.Domain.Enums;

namespace CricketScore.Application.Services;

public class TeamService(ITeamRepository teamRepository, IPlayerRepository playerRepository, IMapper mapper)
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

        var player = new Player
        {
            Name = request.Name,
            Role = request.Role,
            BattingStyle = request.BattingStyle,
            BowlingStyle = request.BowlingStyle,
            DateOfBirth = request.DateOfBirth,
            Nationality = request.Nationality,
            JerseyNumber = request.JerseyNumber,
            CreatedBy = userId
        };

        var createdPlayer = await playerRepository.CreateAsync(player);

        var teamPlayer = new TeamPlayer
        {
            PlayerId = createdPlayer.Id,
            Name = createdPlayer.Name,
            Role = createdPlayer.Role,
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
