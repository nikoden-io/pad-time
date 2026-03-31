// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using FluentValidation;

namespace PadTime.Application.Sites.Commands.CreateSiteSchedule;

/// <summary>
/// Validates <see cref="CreateSiteScheduleCommand"/> ensuring required fields, valid time ranges,
/// and sensible date constraints.
/// </summary>
public sealed class CreateSiteScheduleCommandValidator : AbstractValidator<CreateSiteScheduleCommand>
{
    public CreateSiteScheduleCommandValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Schedule name is required.")
            .MaximumLength(200)
            .WithMessage("Schedule name must not exceed 200 characters.");

        RuleFor(x => x.ValidFrom)
            .NotEmpty()
            .WithMessage("Valid from date is required.");

        RuleFor(x => x.ValidUntil)
            .GreaterThanOrEqualTo(x => x.ValidFrom)
            .When(x => x.ValidUntil.HasValue)
            .WithMessage("Valid until date must be after or equal to valid from date.");

        RuleFor(x => x.OpeningTime)
            .NotEmpty()
            .WithMessage("Opening time is required.");

        RuleFor(x => x.ClosingTime)
            .NotEmpty()
            .WithMessage("Closing time is required.")
            .GreaterThan(x => x.OpeningTime)
            .WithMessage("Closing time must be after opening time.");

        RuleFor(x => x.ApplicableDays)
            .Must(days => days == null || days.Length > 0)
            .WithMessage("Applicable days must contain at least one day if specified.");

        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Priority must be greater than or equal to 0.");
    }
}