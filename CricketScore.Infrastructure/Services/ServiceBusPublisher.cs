using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using CricketScore.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CricketScore.Infrastructure.Services;

public class ServiceBusPublisher(IConfiguration configuration, ILogger<ServiceBusPublisher> logger) : INotificationService, IAsyncDisposable
{
    private ServiceBusClient? _client;
    private readonly string _connectionString = configuration["AzureServiceBus:ConnectionString"] ?? string.Empty;
    private readonly string _topicName = configuration["AzureServiceBus:TopicName"] ?? "match-events";

    public async Task PublishMatchEventAsync(string matchId, string eventType, object payload)
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            logger.LogWarning("ServiceBus connection string is not configured. Event {EventType} for match {MatchId} skipped.", eventType, matchId);
            return;
        }

        try
        {
            _client ??= new ServiceBusClient(_connectionString);
            await using var sender = _client.CreateSender(_topicName);

            var message = new ServiceBusMessage(
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
                {
                    matchId,
                    eventType,
                    payload,
                    timestamp = DateTime.UtcNow
                })))
            {
                Subject = eventType,
                ApplicationProperties = { ["matchId"] = matchId }
            };

            await sender.SendMessageAsync(message);
            logger.LogInformation("Published {EventType} for match {MatchId}", eventType, matchId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish {EventType} for match {MatchId}", eventType, matchId);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_client is not null)
            await _client.DisposeAsync();
    }
}
