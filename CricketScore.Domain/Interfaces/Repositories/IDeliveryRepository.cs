using CricketScore.Domain.Entities;

namespace CricketScore.Domain.Interfaces.Repositories;

public interface IDeliveryRepository
{
    Task<IEnumerable<Delivery>> GetByInningsAsync(string inningsId);
    Task<IEnumerable<Delivery>> GetByInningsAndOverAsync(string inningsId, int overNumber);
    Task<Delivery?> GetLastByInningsAsync(string inningsId);
    Task<Delivery> CreateAsync(Delivery delivery);
    Task DeleteAsync(string inningsId, string deliveryId);
}
