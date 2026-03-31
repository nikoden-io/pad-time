// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.CreateSiteSchedule;

/// <summary>
/// Command to create a new operating schedule for a site, defining opening hours and applicable days.
/// </summary>
/// <param name="SiteId">Identifier of the site.</param>
/// <param name="Name">Display name for the schedule.</param>
/// <param name="ValidFrom">Start date from which the schedule is effective.</param>
/// <param name="ValidUntil">Optional end date. When null, the schedule is open-ended.</param>
/// <param name="OpeningTime">Daily opening time.</param>
/// <param name="ClosingTime">Daily closing time.</param>
/// <param name="ApplicableDays">Optional days of the week when the schedule applies. Null means all days.</param>
/// <param name="Priority">Priority for resolving overlapping schedules (higher wins).</param>
public sealed record CreateSiteScheduleCommand(
    Guid SiteId,
    string Name,
    DateOnly ValidFrom,
    DateOnly? ValidUntil,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime,
    DayOfWeek[]? ApplicableDays,
    int Priority
) : IRequest<Result<Guid>>;