using CricketScore.Application.DTOs.Matches;
using FluentValidation;

namespace CricketScore.Application.Validators;

public class CreateMatchRequestValidator : AbstractValidator<CreateMatchRequest>
{
    public CreateMatchRequestValidator()
    {
        RuleFor(x => x.Team1Id).NotEmpty();
        RuleFor(x => x.Team2Id).NotEmpty().NotEqual(x => x.Team1Id).WithMessage("Teams must be different.");
        RuleFor(x => x.OversPerInnings).GreaterThan(0).LessThanOrEqualTo(50);
        RuleFor(x => x.ScheduledAt).GreaterThan(DateTime.UtcNow.AddMinutes(-5));
        RuleFor(x => x.PlayingXIs).Must(xis => xis.Count == 2).WithMessage("Exactly 2 playing XIs required.");
        RuleForEach(x => x.PlayingXIs).ChildRules(xi =>
        {
            xi.RuleFor(x => x.TeamId).NotEmpty();
            xi.RuleFor(x => x.PlayerIds).Must(ids => ids.Count is >= 11 and <= 11).WithMessage("Each playing XI must have exactly 11 players.");
        });
    }
}
