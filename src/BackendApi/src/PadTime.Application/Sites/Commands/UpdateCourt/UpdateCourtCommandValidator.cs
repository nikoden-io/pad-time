// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using FluentValidation;

namespace PadTime.Application.Sites.Commands.UpdateCourt;

/// <summary>
/// Validates <see cref="UpdateCourtCommand"/> ensuring site/court identifiers and label are provided and within length limits.
/// </summary>
public sealed class UpdateCourtCommandValidator : AbstractValidator<UpdateCourtCommand>
{
    public UpdateCourtCommandValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site ID is required.");

        RuleFor(x => x.CourtId)
            .NotEmpty()
            .WithMessage("Court ID is required.");

        RuleFor(x => x.Label)
            .NotEmpty()
            .WithMessage("Court label is required.")
            .MaximumLength(100)
            .WithMessage("Court label must not exceed 100 characters.");
    }
}