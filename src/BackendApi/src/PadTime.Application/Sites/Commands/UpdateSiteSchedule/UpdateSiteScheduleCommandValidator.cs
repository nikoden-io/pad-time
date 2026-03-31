// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using FluentValidation;

namespace PadTime.Application.Sites.Commands.UpdateSiteSchedule;

/// <summary>
/// Validator for UpdateSiteScheduleCommand with comprehensive business rule validation.
/// </summary>
public sealed class UpdateSiteScheduleCommandValidator : AbstractValidator<UpdateSiteScheduleCommand>
{
    public UpdateSiteScheduleCommandValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site ID is required.");

        RuleFor(x => x.ScheduleId)
            .NotEmpty()
            .WithMessage("Schedule ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Schedule name is required.")
            .MaximumLength(200)
            .WithMessage("Schedule name cannot exceed 200 characters.");

        RuleFor(x => x.ValidFrom)
            .NotEmpty()
            .WithMessage("Valid from date is required.");

        RuleFor(x => x.ValidUntil)
            .Must((command, validUntil) => !validUntil.HasValue || validUntil.Value >= command.ValidFrom)
            .WithMessage("Valid until date must be after or equal to valid from date.");

        RuleFor(x => x.OpeningTime)
            .NotEmpty()
            .WithMessage("Opening time is required.");

        RuleFor(x => x.ClosingTime)
            .NotEmpty()
            .WithMessage("Closing time is required.")
            .Must((command, closingTime) => closingTime > command.OpeningTime)
            .WithMessage("Closing time must be after opening time.");

        RuleFor(x => x.ApplicableDays)
            .Must(days => days == null || days.Length > 0)
            .WithMessage("If applicable days are specified, at least one day must be selected.")
            .Must(days => days == null || days.Distinct().Count() == days.Length)
            .WithMessage("Applicable days cannot contain duplicates.")
            .Must(days => days == null || days.All(day => Enum.IsDefined<DayOfWeek>(day)))
            .WithMessage("All applicable days must be valid days of the week.");

        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Priority must be a non-negative integer.")
            .LessThanOrEqualTo(100)
            .WithMessage("Priority cannot exceed 100.");

        // Business rule: Minimum operating hours (at least 1 hour)
        RuleFor(x => x)
            .Must(command => command.ClosingTime.ToTimeSpan() - command.OpeningTime.ToTimeSpan() >= TimeSpan.FromHours(1))
            .WithMessage("Site must be open for at least 1 hour.")
            .WithName("OperatingHours");

        // Business rule: Reasonable operating hours (not more than 18 hours)
        RuleFor(x => x)
            .Must(command => command.ClosingTime.ToTimeSpan() - command.OpeningTime.ToTimeSpan() <= TimeSpan.FromHours(18))
            .WithMessage("Site cannot be open for more than 18 hours per day.")
            .WithName("OperatingHours");

        // Business rule: Valid from date should not be too far in the past (more than 1 year)
        RuleFor(x => x.ValidFrom)
            .Must(validFrom => validFrom >= DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)))
            .WithMessage("Valid from date cannot be more than 1 year in the past.");

        // Business rule: Valid until date should not be too far in the future (more than 5 years)
        RuleFor(x => x.ValidUntil)
            .Must(validUntil => !validUntil.HasValue || validUntil.Value <= DateOnly.FromDateTime(DateTime.UtcNow.AddYears(5)))
            .WithMessage("Valid until date cannot be more than 5 years in the future.");
    }
}