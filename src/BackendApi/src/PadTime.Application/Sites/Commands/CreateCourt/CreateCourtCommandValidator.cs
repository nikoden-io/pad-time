using FluentValidation;

namespace PadTime.Application.Sites.Commands.CreateCourt;

public sealed class CreateCourtCommandValidator : AbstractValidator<CreateCourtCommand>
{
    public CreateCourtCommandValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site ID is required.");

        RuleFor(x => x.Label)
            .NotEmpty()
            .WithMessage("Court label is required.")
            .MaximumLength(100)
            .WithMessage("Court label must not exceed 100 characters.");
    }
}
