using CricketScore.Domain.Interfaces.Repositories;
using CricketScore.Infrastructure.Persistence;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using DomainUser = CricketScore.Domain.Entities.User;

namespace CricketScore.Infrastructure.Repositories;

public class UserRepository(CosmosDbContext context) : IUserRepository
{
    private Container Container => context.Users;

    public async Task<DomainUser?> GetByIdAsync(string id)
    {
        try
        {
            var response = await Container.ReadItemAsync<DomainUser>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<DomainUser?> GetByEmailAsync(string email)
    {
        var query = Container.GetItemLinqQueryable<DomainUser>()
            .Where(u => u.Email == email)
            .ToFeedIterator();

        while (query.HasMoreResults)
        {
            var page = await query.ReadNextAsync();
            var user = page.FirstOrDefault();
            if (user != null) return user;
        }

        return null;
    }

    public async Task<DomainUser> CreateAsync(DomainUser user)
    {
        var response = await Container.CreateItemAsync(user, new PartitionKey(user.Id));
        return response.Resource;
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await GetByEmailAsync(email) != null;
    }
}
