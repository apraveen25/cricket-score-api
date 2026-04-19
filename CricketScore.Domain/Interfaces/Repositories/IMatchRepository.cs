using CricketScore.Domain.Entities;

namespace CricketScore.Domain.Interfaces.Repositories;

public interface IMatchRepository
{
    Task<Match?> GetByIdAsync(string id);
    Task<IEnumerable<Match>> GetRecentAsync(int count = 20);
    Task<Match> CreateAsync(Match match);
    Task<Match> UpdateAsync(Match match);
}
