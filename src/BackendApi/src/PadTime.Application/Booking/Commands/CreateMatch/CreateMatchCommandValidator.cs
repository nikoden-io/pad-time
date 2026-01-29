using FluentValidation;

namespace PadTime.Application.Booking.Commands.CreateMatch;

public sealed class CreateMatchCommandValidator : AbstractValidator<CreateMatchCommand>
{
    public CreateMatchCommandValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site ID is required.");

        RuleFor(x => x.CourtId)
            .NotEmpty()
            .WithMessage("Court ID is required.");

        RuleFor(x => x.StartAtUtc)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Start time must be in the future.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid match type.");

        RuleFor(x => x.PrivateParticipantMatricules)
            .Must(x => x == null || x.Count <= 3)
            .WithMessage("Maximum 3 additional participants allowed.");
    }
}
