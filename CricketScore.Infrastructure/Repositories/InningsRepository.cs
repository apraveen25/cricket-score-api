using CricketScore.Domain.Entities;
using CricketScore.Domain.Interfaces.Repositories;
using CricketScore.Infrastructure.Persistence;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace CricketScore.Infrastructure.Repositories;

public class InningsRepository(CosmosDbContext context) : IInningsRepository
{
    private Container Container => context.Innings;

    public async Task<Innings?> GetByIdAsync(string id, string matchId)
    {
        try
        {
            var response = await Container.ReadItemAsync<Innings>(id, new PartitionKey(matchId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Innings>> GetByMatchAsync(string matchId)
    {
        var results = new List<Innings>();
        var query = Container.GetItemLinqQueryable<Innings>()
            .Where(i => i.MatchId == matchId)
            .ToFeedIterator();

        while (query.HasMoreResults)
        {
            var page = await query.ReadNextAsync();
            results.AddRange(page);
        }

        return results;
    }

    public async Task<Innings> CreateAsync(Innings innings)
    {
        var response = await Container.CreateItemAsync(innings, new PartitionKey(innings.MatchId));
        return response.Resource;
    }

    public async Task<Innings> UpdateAsync(Innings innings)
    {
        var response = await Container.ReplaceItemAsync(innings, innings.Id, new PartitionKey(innings.MatchId));
        return response.Resource;
    }
}
