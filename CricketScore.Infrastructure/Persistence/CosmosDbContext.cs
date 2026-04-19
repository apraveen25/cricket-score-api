using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace CricketScore.Infrastructure.Persistence;

public class CosmosDbContext
{
    private readonly CosmosClient _client;
    private readonly CosmosDbSettings _settings;

    public CosmosDbContext(CosmosClient client, IOptions<CosmosDbSettings> settings)
    {
        _client = client;
        _settings = settings.Value;
    }

    public Container Users => _client.GetContainer(_settings.DatabaseName, _settings.UsersContainer);
    public Container Teams => _client.GetContainer(_settings.DatabaseName, _settings.TeamsContainer);
    public Container Matches => _client.GetContainer(_settings.DatabaseName, _settings.MatchesContainer);
    public Container Innings => _client.GetContainer(_settings.DatabaseName, _settings.InningsContainer);
    public Container Deliveries => _client.GetContainer(_settings.DatabaseName, _settings.DeliveriesContainer);
    public Container Players => _client.GetContainer(_settings.DatabaseName, _settings.PlayersContainer);

    public async Task InitializeAsync()
    {
        var db = await _client.CreateDatabaseIfNotExistsAsync(_settings.DatabaseName);

        await db.Database.CreateContainerIfNotExistsAsync(_settings.UsersContainer, "/id");
        await db.Database.CreateContainerIfNotExistsAsync(_settings.TeamsContainer, "/id");
        await db.Database.CreateContainerIfNotExistsAsync(_settings.MatchesContainer, "/id");
        await db.Database.CreateContainerIfNotExistsAsync(_settings.InningsContainer, "/matchId");
        await db.Database.CreateContainerIfNotExistsAsync(_settings.DeliveriesContainer, "/inningsId");
        await db.Database.CreateContainerIfNotExistsAsync(_settings.PlayersContainer, "/id");
    }
}
