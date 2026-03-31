// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using FluentValidation;

namespace PadTime.Application.Sites.Commands.UpdateSite;

/// <summary>
/// Validates <see cref="UpdateSiteCommand"/> ensuring all address fields are present, within length limits,
/// and the timezone is a valid IANA identifier.
/// </summary>
public sealed class UpdateSiteCommandValidator : AbstractValidator<UpdateSiteCommand>
{
    public UpdateSiteCommandValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Site name is required.")
            .MaximumLength(200)
            .WithMessage("Site name must not exceed 200 characters.");

        RuleFor(x => x.StreetNumber)
            .NotEmpty()
            .WithMessage("Street number is required.")
            .MaximumLength(15)
            .WithMessage("Street number must not exceed 15 characters.");

        RuleFor(x => x.Street)
            .NotEmpty()
            .WithMessage("Street is required.")
            .MaximumLength(200)
            .WithMessage("Street must not exceed 200 characters.");

        RuleFor(x => x.Postcode)
            .NotEmpty()
            .WithMessage("Post code is required.")
            .MaximumLength(10)
            .WithMessage("Post code must not exceed 10 characters.");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required.")
            .MaximumLength(200)
            .WithMessage("City must not exceed 200 characters.");

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("Country is required.")
            .MaximumLength(200)
            .WithMessage("Country must not exceed 200 characters.");

        RuleFor(x => x.Timezone)
            .NotEmpty()
            .WithMessage("Timezone is required.")
            .Must(BeValidTimezone)
            .WithMessage("Invalid timezone identifier. Use IANA timezone format (e.g., 'Europe/Brussels').");
    }

    private static bool BeValidTimezone(string timezone)
    {
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timezone);
            return true;
        }
        catch
        {
            return false;
        }
    }
}