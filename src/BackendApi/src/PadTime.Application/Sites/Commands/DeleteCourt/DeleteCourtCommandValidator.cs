using FluentValidation;

namespace PadTime.Application.Sites.Commands.DeleteCourt;

public sealed class DeleteCourtCommandValidator : AbstractValidator<DeleteCourtCommand>
{
    public DeleteCourtCommandValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site ID is required.");

        RuleFor(x => x.CourtId)
            .NotEmpty()
            .WithMessage("Court ID is required.");
    }
}
