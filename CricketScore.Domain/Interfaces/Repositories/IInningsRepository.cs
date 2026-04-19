using CricketScore.Domain.Entities;

namespace CricketScore.Domain.Interfaces.Repositories;

public interface IInningsRepository
{
    Task<Innings?> GetByIdAsync(string id, string matchId);
    Task<IEnumerable<Innings>> GetByMatchAsync(string matchId);
    Task<Innings> CreateAsync(Innings innings);
    Task<Innings> UpdateAsync(Innings innings);
}
