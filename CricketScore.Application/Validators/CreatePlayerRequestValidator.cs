using CricketScore.Application.DTOs.Players;
using FluentValidation;

namespace CricketScore.Application.Validators;

public class CreatePlayerRequestValidator : AbstractValidator<CreatePlayerRequest>
{
    public CreatePlayerRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Role)
            .IsInEnum();

        RuleFor(x => x.BattingStyle)
            .IsInEnum();

        RuleFor(x => x.BowlingStyle)
            .IsInEnum();

        RuleFor(x => x.DateOfBirth)
            .Must(dob => dob == null || dob < DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date of birth must be in the past.");

        RuleFor(x => x.Nationality)
            .MaximumLength(100)
            .When(x => x.Nationality != null);

        RuleFor(x => x.JerseyNumber)
            .InclusiveBetween(0, 999)
            .When(x => x.JerseyNumber.HasValue);
    }
}
