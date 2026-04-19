using CricketScore.Domain.Entities;

namespace CricketScore.Domain.Interfaces.Repositories;

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(string id);
    Task<IEnumerable<Player>> GetAllAsync();
    Task<IEnumerable<Player>> SearchByNameAsync(string name);
    Task<Player> CreateAsync(Player player);
    Task<Player> UpdateAsync(Player player);
}
