using CricketScore.Application.DTOs.Players;
using CricketScore.Domain.Entities;
using CricketScore.Domain.Interfaces.Repositories;

namespace CricketScore.Application.Services;

public class PlayerService(IPlayerRepository playerRepository)
{
    public async Task<PlayerResponse> CreatePlayerAsync(CreatePlayerRequest request, string userId)
    {
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

        var created = await playerRepository.CreateAsync(player);
        return ToResponse(created);
    }

    public async Task<PlayerResponse> GetPlayerAsync(string id)
    {
        var player = await playerRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Player {id} not found.");

        return ToResponse(player);
    }

    public async Task<PlayerResponse> UpdatePlayerAsync(string id, UpdatePlayerRequest request, string userId)
    {
        var player = await playerRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Player {id} not found.");

        if (player.CreatedBy != userId)
            throw new UnauthorizedAccessException("Only the player's creator can update this profile.");

        if (request.Name != null) player.Name = request.Name;
        if (request.Role.HasValue) player.Role = request.Role.Value;
        if (request.BattingStyle.HasValue) player.BattingStyle = request.BattingStyle.Value;
        if (request.BowlingStyle.HasValue) player.BowlingStyle = request.BowlingStyle.Value;
        if (request.DateOfBirth.HasValue) player.DateOfBirth = request.DateOfBirth;
        if (request.Nationality != null) player.Nationality = request.Nationality;
        if (request.JerseyNumber.HasValue) player.JerseyNumber = request.JerseyNumber;

        var updated = await playerRepository.UpdateAsync(player);
        return ToResponse(updated);
    }

    public async Task<IEnumerable<PlayerResponse>> GetAllPlayersAsync()
    {
        var players = await playerRepository.GetAllAsync();
        return players.Select(ToResponse);
    }

    public async Task<IEnumerable<PlayerResponse>> SearchPlayersAsync(string name)
    {
        var players = await playerRepository.SearchByNameAsync(name);
        return players.Select(ToResponse);
    }

    private static PlayerResponse ToResponse(Player p) => new(
        p.Id, p.Name, p.Role, p.BattingStyle, p.BowlingStyle,
        p.DateOfBirth, p.Nationality, p.JerseyNumber, p.CreatedBy, p.CreatedAt);
}
