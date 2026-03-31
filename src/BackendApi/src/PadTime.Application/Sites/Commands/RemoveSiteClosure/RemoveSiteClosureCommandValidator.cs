// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using FluentValidation;

namespace PadTime.Application.Sites.Commands.RemoveSiteClosure;

/// <summary>
/// Validator for RemoveSiteClosureCommand.
/// </summary>
public sealed class RemoveSiteClosureCommandValidator : AbstractValidator<RemoveSiteClosureCommand>
{
    public RemoveSiteClosureCommandValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site ID is required.");

        RuleFor(x => x.ClosureId)
            .NotEmpty()
            .WithMessage("Closure ID is required.");
    }
}