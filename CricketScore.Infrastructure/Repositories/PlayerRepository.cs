using CricketScore.Domain.Entities;
using CricketScore.Domain.Interfaces.Repositories;
using CricketScore.Infrastructure.Persistence;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace CricketScore.Infrastructure.Repositories;

public class PlayerRepository(CosmosDbContext context) : IPlayerRepository
{
    private Container Container => context.Players;

    public async Task<Player?> GetByIdAsync(string id)
    {
        try
        {
            var response = await Container.ReadItemAsync<Player>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Player>> GetAllAsync()
    {
        var results = new List<Player>();
        var query = Container.GetItemLinqQueryable<Player>().ToFeedIterator();

        while (query.HasMoreResults)
        {
            var page = await query.ReadNextAsync();
            results.AddRange(page);
        }

        return results;
    }

    public async Task<IEnumerable<Player>> SearchByNameAsync(string name)
    {
        var results = new List<Player>();
        var lower = name.ToLower();

        var query = Container.GetItemLinqQueryable<Player>()
            .Where(p => p.Name.ToLower().Contains(lower))
            .ToFeedIterator();

        while (query.HasMoreResults)
        {
            var page = await query.ReadNextAsync();
            results.AddRange(page);
        }

        return results;
    }

    public async Task<Player> CreateAsync(Player player)
    {
        var response = await Container.CreateItemAsync(player, new PartitionKey(player.Id));
        return response.Resource;
    }

    public async Task<Player> UpdateAsync(Player player)
    {
        var response = await Container.ReplaceItemAsync(player, player.Id, new PartitionKey(player.Id));
        return response.Resource;
    }
}
