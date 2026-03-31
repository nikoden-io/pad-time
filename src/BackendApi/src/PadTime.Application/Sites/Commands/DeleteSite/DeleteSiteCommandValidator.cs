// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using FluentValidation;

namespace PadTime.Application.Sites.Commands.DeleteSite;

/// <summary>
/// Validates <see cref="DeleteSiteCommand"/> ensuring the site identifier is provided.
/// </summary>
public class DeleteSiteCommandValidator : AbstractValidator<DeleteSiteCommand>
{
    public DeleteSiteCommandValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site ID is required.");
    }
}