// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Queries.GetSiteSchedule;

/// <summary>
/// Query to get site schedule information including regular schedules and closures.
/// </summary>
public sealed record GetSiteScheduleQuery(Guid SiteId) : IRequest<Result<SiteScheduleDetailDto>>;

/// <summary>
/// DTO representing comprehensive site schedule information.
/// </summary>
public sealed record SiteScheduleDetailDto(
    Guid SiteId,
    string SiteName,
    string Timezone,
    IReadOnlyList<SiteScheduleDto> Schedules,
    IReadOnlyList<SiteClosureDto> Closures
);