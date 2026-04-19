using CricketScore.Domain.Interfaces.Repositories;
using CricketScore.Domain.Interfaces.Services;
using CricketScore.Infrastructure.Persistence;
using CricketScore.Infrastructure.Repositories;
using CricketScore.Infrastructure.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CricketScore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CosmosDbSettings>(configuration.GetSection("CosmosDb"));

        services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<CosmosDbSettings>>().Value;
            return new CosmosClient(settings.ConnectionString, new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            });
        });

        services.AddSingleton<CosmosDbContext>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();
        services.AddScoped<IInningsRepository, InningsRepository>();
        services.AddScoped<IDeliveryRepository, DeliveryRepository>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();

        services.AddScoped<ITokenService, TokenService>();
        services.AddSingleton<INotificationService, ServiceBusPublisher>();

        return services;
    }
}
