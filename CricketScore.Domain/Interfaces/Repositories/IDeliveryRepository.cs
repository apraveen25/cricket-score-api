using CricketScore.Domain.Entities;

namespace CricketScore.Domain.Interfaces.Repositories;

public interface IDeliveryRepository
{
    Task<IEnumerable<Delivery>> GetByInningsAsync(string inningsId);
    Task<Delivery> CreateAsync(Delivery delivery);
}
