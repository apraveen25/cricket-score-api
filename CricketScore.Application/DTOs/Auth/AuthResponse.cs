namespace CricketScore.Application.DTOs.Auth;

public record AuthResponse(
    string UserId,
    string Name,
    string Email,
    string Role,
    string Token,
    DateTime ExpiresAt
);
