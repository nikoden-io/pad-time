// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using FluentValidation;

namespace PadTime.Application.Sites.Commands.DeleteSiteSchedule;

/// <summary>
/// Validates <see cref="DeleteSiteScheduleCommand"/> ensuring both site and schedule identifiers are provided.
/// </summary>
public sealed class DeleteSiteScheduleCommandValidator : AbstractValidator<DeleteSiteScheduleCommand>
{
    public DeleteSiteScheduleCommandValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site ID is required.");

        RuleFor(x => x.ScheduleId)
            .NotEmpty()
            .WithMessage("Schedule ID is required.");
    }
}