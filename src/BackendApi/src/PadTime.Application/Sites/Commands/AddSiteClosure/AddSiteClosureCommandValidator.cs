// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using FluentValidation;
using PadTime.Domain.Site;

namespace PadTime.Application.Sites.Commands.AddSiteClosure;

/// <summary>
/// Validator for AddSiteClosureCommand with comprehensive business rule validation.
/// </summary>
public sealed class AddSiteClosureCommandValidator : AbstractValidator<AddSiteClosureCommand>
{
    public AddSiteClosureCommandValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site ID is required.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Closure type must be a valid value.");

        RuleFor(x => x.Reason)
            .IsInEnum()
            .WithMessage("Closure reason must be a valid value.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required.")
            .Must(startDate => startDate >= DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Start date cannot be in the past.");

        // End date validation for Period closures
        RuleFor(x => x.EndDate)
            .Must((command, endDate) => command.Type != ClosureType.Period || endDate.HasValue)
            .WithMessage("End date is required for period closures.")
            .Must((command, endDate) => !endDate.HasValue || endDate.Value >= command.StartDate)
            .WithMessage("End date must be after or equal to start date.");

        // Modified hours validation for ReducedHours closures
        RuleFor(x => x.ModifiedOpeningTime)
            .Must((command, openingTime) => command.Type != ClosureType.ReducedHours || openingTime.HasValue)
            .WithMessage("Modified opening time is required for reduced hours closures.");

        RuleFor(x => x.ModifiedClosingTime)
            .Must((command, closingTime) => command.Type != ClosureType.ReducedHours || closingTime.HasValue)
            .WithMessage("Modified closing time is required for reduced hours closures.");

        RuleFor(x => x)
            .Must(command => command.Type != ClosureType.ReducedHours || 
                           !command.ModifiedOpeningTime.HasValue || 
                           !command.ModifiedClosingTime.HasValue ||
                           command.ModifiedClosingTime.Value > command.ModifiedOpeningTime.Value)
            .WithMessage("Modified closing time must be after modified opening time for reduced hours closures.")
            .WithName("ModifiedHours");

        // Business rule: Closure period should not exceed 1 year
        RuleFor(x => x)
            .Must(command => command.Type != ClosureType.Period || 
                           !command.EndDate.HasValue ||
                           command.EndDate.Value.ToDateTime(TimeOnly.MinValue) - command.StartDate.ToDateTime(TimeOnly.MinValue) <= TimeSpan.FromDays(365))
            .WithMessage("Closure period cannot exceed 1 year.")
            .WithName("ClosurePeriod");

        // Business rule: Future closure should not be scheduled more than 2 years in advance
        RuleFor(x => x.StartDate)
            .Must(startDate => startDate <= DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2)))
            .WithMessage("Closure cannot be scheduled more than 2 years in advance.");

        RuleFor(x => x.AffectedCourtIds)
            .Must(courtIds => courtIds == null || courtIds.Length > 0)
            .WithMessage("If affected courts are specified, at least one court must be selected.")
            .Must(courtIds => courtIds == null || courtIds.Distinct().Count() == courtIds.Length)
            .WithMessage("Affected court IDs cannot contain duplicates.");
    }
}