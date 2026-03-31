// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using FluentValidation;

namespace PadTime.Application.Sites.Commands.ActivateSite;

/// <summary>
/// Validates <see cref="ActivateSiteCommand"/> ensuring the site identifier is provided.
/// </summary>
public class ActivateSiteCommandValidator : AbstractValidator<ActivateSiteCommand>
{
    public ActivateSiteCommandValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site ID is required.");
    }
}