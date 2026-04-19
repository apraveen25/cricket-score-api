using CricketScore.Application.DTOs.Auth;
using CricketScore.Application.DTOs.Matches;
using CricketScore.Application.DTOs.Players;
using CricketScore.Application.DTOs.Scoring;
using CricketScore.Application.Mappings;
using CricketScore.Application.Services;
using CricketScore.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CricketScore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

        services.AddScoped<AuthService>();
        services.AddScoped<TeamService>();
        services.AddScoped<MatchService>();
        services.AddScoped<ScoringService>();
        services.AddScoped<PlayerService>();

        services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
        services.AddScoped<IValidator<CreateMatchRequest>, CreateMatchRequestValidator>();
        services.AddScoped<IValidator<BallDeliveryRequest>, BallDeliveryRequestValidator>();
        services.AddScoped<IValidator<CreatePlayerRequest>, CreatePlayerRequestValidator>();

        return services;
    }
}
