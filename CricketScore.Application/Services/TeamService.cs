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
        ResequenceSerials(team);
        var updated = await teamRepository.UpdateAsync(team);
        return mapper.Map<TeamResponse>(updated);
    }

    public async Task<TeamResponse> RemovePlayerAsync(string teamId, string playerId, string userId)
    {
        var team = await teamRepository.GetByIdAsync(teamId)
            ?? throw new KeyNotFoundException($"Team {teamId} not found.");

        if (team.CreatedBy != userId)
            throw new UnauthorizedAccessException("Only the team creator can remove players.");

        var player = team.Players.FirstOrDefault(p => p.PlayerId == playerId)
            ?? throw new KeyNotFoundException($"Player {playerId} is not in this team.");

        team.Players.Remove(player);
        ResequenceSerials(team);
        var updated = await teamRepository.UpdateAsync(team);
        return mapper.Map<TeamResponse>(updated);
    }

    public async Task<TeamResponse> UpdateTeamPlayerAsync(string teamId, string playerId, UpdateTeamPlayerRequest request, string userId)
    {
        var team = await teamRepository.GetByIdAsync(teamId)
            ?? throw new KeyNotFoundException($"Team {teamId} not found.");

        if (team.CreatedBy != userId)
            throw new UnauthorizedAccessException("Only the team creator can edit players.");

        var player = team.Players.FirstOrDefault(p => p.PlayerId == playerId)
            ?? throw new KeyNotFoundException($"Player {playerId} is not in this team.");

        if (request.Name is not null) player.Name = request.Name;
        if (request.Role.HasValue) player.Role = request.Role.Value;
        if (request.IsCaptain.HasValue) player.IsCaptain = request.IsCaptain.Value;
        if (request.IsWicketKeeper.HasValue) player.IsWicketKeeper = request.IsWicketKeeper.Value;

        var updated = await teamRepository.UpdateAsync(team);
        return mapper.Map<TeamResponse>(updated);
    }

    public async Task<IEnumerable<TeamResponse>> GetMyTeamsAsync(string userId)
    {
        var teams = await teamRepository.GetByCreatorAsync(userId);
        return mapper.Map<IEnumerable<TeamResponse>>(teams);
    }

    public async Task<TeamResponse> RearrangePlayersAsync(string teamId, RearrangePlayersRequest request, string userId)
    {
        var team = await teamRepository.GetByIdAsync(teamId)
            ?? throw new KeyNotFoundException($"Team {teamId} not found.");

        if (team.CreatedBy != userId)
            throw new UnauthorizedAccessException("Only the team creator can rearrange players.");

        var incoming = request.PlayerIds.Distinct().ToList();

        if (incoming.Count != team.Players.Count ||
            !incoming.All(id => team.Players.Any(p => p.PlayerId == id)))
            throw new ArgumentException("PlayerIds must contain every player in the team exactly once.");

        for (int i = 0; i < incoming.Count; i++)
            team.Players.First(p => p.PlayerId == incoming[i]).SerialNo = i + 1;

        var updated = await teamRepository.UpdateAsync(team);
        return mapper.Map<TeamResponse>(updated);
    }

    private static void ResequenceSerials(Team team)
    {
        var ordered = team.Players.OrderBy(p => p.SerialNo).ToList();
        for (int i = 0; i < ordered.Count; i++)
            ordered[i].SerialNo = i + 1;
    }
}
