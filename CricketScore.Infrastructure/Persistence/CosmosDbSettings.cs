namespace CricketScore.Infrastructure.Persistence;

public class CosmosDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "appdb";
    public string UsersContainer { get; set; } = "users";
    public string TeamsContainer { get; set; } = "teams";
    public string MatchesContainer { get; set; } = "matches";
    public string InningsContainer { get; set; } = "innings";
    public string DeliveriesContainer { get; set; } = "deliveries";
}
