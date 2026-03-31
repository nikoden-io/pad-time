// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.UpdateSiteSchedule;

/// <summary>
/// Command to update an existing site operating schedule.
/// </summary>
/// <param name="SiteId">Identifier of the site.</param>
/// <param name="ScheduleId">Identifier of the schedule to update.</param>
/// <param name="Name">Updated schedule name.</param>
/// <param name="ValidFrom">Updated validity start date.</param>
/// <param name="ValidUntil">Updated optional end date.</param>
/// <param name="OpeningTime">Updated daily opening time.</param>
/// <param name="ClosingTime">Updated daily closing time.</param>
/// <param name="ApplicableDays">Updated applicable days of the week.</param>
/// <param name="Priority">Updated priority for schedule resolution.</param>
public sealed record UpdateSiteScheduleCommand(
    Guid SiteId,
    Guid ScheduleId,
    string Name,
    DateOnly ValidFrom,
    DateOnly? ValidUntil,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime,
    DayOfWeek[]? ApplicableDays,
    int Priority
) : IRequest<Result>;