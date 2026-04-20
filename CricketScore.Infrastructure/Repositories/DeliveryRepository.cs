using CricketScore.Domain.Entities;
using CricketScore.Domain.Interfaces.Repositories;
using CricketScore.Infrastructure.Persistence;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace CricketScore.Infrastructure.Repositories;

public class DeliveryRepository(CosmosDbContext context) : IDeliveryRepository
{
    private Container Container => context.Deliveries;

    public async Task<IEnumerable<Delivery>> GetByInningsAsync(string inningsId)
    {
        var results = new List<Delivery>();
        var query = Container.GetItemLinqQueryable<Delivery>()
            .Where(d => d.InningsId == inningsId)
            .OrderBy(d => d.OverNumber).ThenBy(d => d.BallInOver)
            .ToFeedIterator();

        while (query.HasMoreResults)
        {
            var page = await query.ReadNextAsync();
            results.AddRange(page);
        }

        return results;
    }

    public async Task<IEnumerable<Delivery>> GetByInningsAndOverAsync(string inningsId, int overNumber)
    {
        var results = new List<Delivery>();
        var query = Container.GetItemLinqQueryable<Delivery>()
            .Where(d => d.InningsId == inningsId && d.OverNumber == overNumber)
            .OrderBy(d => d.Timestamp)
            .ToFeedIterator();

        while (query.HasMoreResults)
        {
            var page = await query.ReadNextAsync();
            results.AddRange(page);
        }

        return results;
    }

    public async Task<Delivery?> GetLastByInningsAsync(string inningsId)
    {
        var results = new List<Delivery>();
        var query = Container.GetItemLinqQueryable<Delivery>()
            .Where(d => d.InningsId == inningsId)
            .OrderByDescending(d => d.Timestamp)
            .Take(1)
            .ToFeedIterator();

        while (query.HasMoreResults)
        {
            var page = await query.ReadNextAsync();
            results.AddRange(page);
        }

        return results.FirstOrDefault();
    }

    public async Task<Delivery> CreateAsync(Delivery delivery)
    {
        var response = await Container.CreateItemAsync(delivery, new PartitionKey(delivery.InningsId));
        return response.Resource;
    }

    public async Task DeleteAsync(string inningsId, string deliveryId)
    {
        await Container.DeleteItemAsync<Delivery>(deliveryId, new PartitionKey(inningsId));
    }
}
