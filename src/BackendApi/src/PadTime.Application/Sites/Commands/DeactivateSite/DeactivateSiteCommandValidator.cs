// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using FluentValidation;

namespace PadTime.Application.Sites.Commands.DeactivateSite;

/// <summary>
/// Validates <see cref="DeactivateSiteCommand"/> ensuring the site identifier is provided.
/// </summary>
public class DeactivateSiteCommandValidator : AbstractValidator<DeactivateSiteCommand>
{
    public DeactivateSiteCommandValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site ID is required.");
    }
}