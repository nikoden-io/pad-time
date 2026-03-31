// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using FluentValidation;

namespace PadTime.Application.Sites.Commands.DeleteCourt;

/// <summary>
/// Validates <see cref="DeleteCourtCommand"/> ensuring both site and court identifiers are provided.
/// </summary>
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