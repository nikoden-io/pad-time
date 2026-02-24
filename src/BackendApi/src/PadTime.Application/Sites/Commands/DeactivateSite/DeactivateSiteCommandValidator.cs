using FluentValidation;

namespace PadTime.Application.Sites.Commands.DeactivateSite;

public class DeactivateSiteCommandValidator : AbstractValidator<DeactivateSiteCommand>
{
    public DeactivateSiteCommandValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site ID is required.");
    }
}