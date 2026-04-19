using CricketScore.Application.DTOs.Scoring;
using CricketScore.Domain.Enums;
using FluentValidation;

namespace CricketScore.Application.Validators;

public class BallDeliveryRequestValidator : AbstractValidator<BallDeliveryRequest>
{
    public BallDeliveryRequestValidator()
    {
        RuleFor(x => x.BatsmanId).NotEmpty();
        RuleFor(x => x.BowlerId).NotEmpty();
        RuleFor(x => x.RunsScored).GreaterThanOrEqualTo(0).LessThanOrEqualTo(6);
        RuleFor(x => x.ExtraRuns).GreaterThanOrEqualTo(0).LessThanOrEqualTo(5);
        RuleFor(x => x.WicketType)
            .NotEqual(WicketType.None)
            .When(x => x.IsWicket)
            .WithMessage("WicketType must be specified when IsWicket is true.");
        RuleFor(x => x.DismissedBatsmanId)
            .NotEmpty()
            .When(x => x.IsWicket)
            .WithMessage("DismissedBatsmanId is required for a wicket.");
    }
}
