using CricketScore.Domain.Entities;
using CricketScore.Domain.Interfaces.Repositories;
using CricketScore.Infrastructure.Persistence;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace CricketScore.Infrastructure.Repositories;

public class TeamRepository(CosmosDbContext context) : ITeamRepository
{
    private Container Container => context.Teams;

    public async Task<Team?> GetByIdAsync(string id)
    {
        try
        {
            var response = await Container.ReadItemAsync<Team>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Team>> GetByCreatorAsync(string userId)
    {
        var results = new List<Team>();
        var query = Container.GetItemLinqQueryable<Team>()
            .Where(t => t.CreatedBy == userId)
            .ToFeedIterator();

        while (query.HasMoreResults)
        {
            var page = await query.ReadNextAsync();
            results.AddRange(page);
        }

        return results;
    }

    public async Task<Team> CreateAsync(Team team)
    {
        var response = await Container.CreateItemAsync(team, new PartitionKey(team.Id));
        return response.Resource;
    }

    public async Task<Team> UpdateAsync(Team team)
    {
        var response = await Container.ReplaceItemAsync(team, team.Id, new PartitionKey(team.Id));
        return response.Resource;
    }
}
