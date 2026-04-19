using CricketScore.Application.DTOs.Auth;
using CricketScore.Domain.Entities;
using CricketScore.Domain.Interfaces.Repositories;
using CricketScore.Domain.Interfaces.Services;

namespace CricketScore.Application.Services;

public class AuthService(IUserRepository userRepository, ITokenService tokenService)
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await userRepository.ExistsByEmailAsync(request.Email))
            throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            Name = request.Name,
            Email = request.Email.ToLowerInvariant(),
            Phone = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        var created = await userRepository.CreateAsync(user);
        var token = tokenService.GenerateToken(created);

        return new AuthResponse(
            created.Id,
            created.Name,
            created.Email,
            created.Role.ToString(),
            token,
            DateTime.UtcNow.AddHours(24)
        );
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email.ToLowerInvariant())
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = tokenService.GenerateToken(user);

        return new AuthResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Role.ToString(),
            token,
            DateTime.UtcNow.AddHours(24)
        );
    }
}
