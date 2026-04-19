using CricketScore.Domain.Entities;
using CricketScore.Domain.Interfaces.Repositories;
using CricketScore.Infrastructure.Persistence;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace CricketScore.Infrastructure.Repositories;

public class MatchRepository(CosmosDbContext context) : IMatchRepository
{
    private Container Container => context.Matches;

    public async Task<Match?> GetByIdAsync(string id)
    {
        try
        {
            var response = await Container.ReadItemAsync<Match>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Match>> GetRecentAsync(int count = 20)
    {
        var results = new List<Match>();
        var query = Container.GetItemLinqQueryable<Match>()
            .OrderByDescending(m => m.CreatedAt)
            .Take(count)
            .ToFeedIterator();

        while (query.HasMoreResults)
        {
            var page = await query.ReadNextAsync();
            results.AddRange(page);
        }

        return results;
    }

    public async Task<Match> CreateAsync(Match match)
    {
        var response = await Container.CreateItemAsync(match, new PartitionKey(match.Id));
        return response.Resource;
    }

    public async Task<Match> UpdateAsync(Match match)
    {
        var response = await Container.ReplaceItemAsync(match, match.Id, new PartitionKey(match.Id));
        return response.Resource;
    }
}
