// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.DeleteSiteSchedule;

/// <summary>
/// Command to delete an operating schedule from a site.
/// </summary>
/// <param name="SiteId">Identifier of the site.</param>
/// <param name="ScheduleId">Identifier of the schedule to remove.</param>
public sealed record DeleteSiteScheduleCommand(
    Guid SiteId,
    Guid ScheduleId) : IRequest<Result>;