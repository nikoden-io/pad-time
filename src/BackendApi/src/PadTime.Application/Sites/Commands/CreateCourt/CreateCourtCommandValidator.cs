// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using FluentValidation;

namespace PadTime.Application.Sites.Commands.CreateCourt;

/// <summary>
/// Validates <see cref="CreateCourtCommand"/> ensuring site identifier and court label are provided and within length limits.
/// </summary>
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