using FluentValidation;

namespace PadTime.Application.Sites.Commands.ActivateSite;

public class ActivateSiteCommandValidator : AbstractValidator<ActivateSiteCommand>
{
    public ActivateSiteCommandValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site ID is required.");
    }
}