using CricketScore.Domain.Entities;

namespace CricketScore.Domain.Interfaces.Repositories;

public interface ITeamRepository
{
    Task<Team?> GetByIdAsync(string id);
    Task<IEnumerable<Team>> GetByCreatorAsync(string userId);
    Task<Team> CreateAsync(Team team);
    Task<Team> UpdateAsync(Team team);
}
