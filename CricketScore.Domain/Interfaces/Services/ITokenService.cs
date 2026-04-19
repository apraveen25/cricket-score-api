using CricketScore.Domain.Entities;

namespace CricketScore.Domain.Interfaces.Services;

public interface ITokenService
{
    string GenerateToken(User user);
}
