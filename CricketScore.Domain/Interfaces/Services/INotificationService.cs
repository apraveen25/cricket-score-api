namespace CricketScore.Domain.Interfaces.Services;

public interface INotificationService
{
    Task PublishMatchEventAsync(string matchId, string eventType, object payload);
}
