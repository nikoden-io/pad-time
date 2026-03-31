// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Queries.GetSiteSchedule;

/// <summary>
/// Handler for retrieving comprehensive site schedule information.
/// </summary>
public sealed class GetSiteScheduleQueryHandler(ISiteRepository siteRepository)
    : IRequestHandler<GetSiteScheduleQuery, Result<SiteScheduleDetailDto>>
{
    public async Task<Result<SiteScheduleDetailDto>> Handle(
        GetSiteScheduleQuery request,
        CancellationToken cancellationToken)
    {
        var site = await siteRepository.GetByIdWithSchedulesAndClosuresAsync(request.SiteId, cancellationToken);
        if (site is null)
            return DomainErrors.Site.NotFound;

        var schedules = site.Schedules
            .OrderBy(s => s.Priority)
            .ThenBy(s => s.ValidFrom)
            .Select(s => new SiteScheduleDto(
                ScheduleId: s.Id,
                Name: s.Name,
                ValidFrom: s.ValidFrom,
                ValidUntil: s.ValidUntil,
                OpeningTime: s.OpeningTime,
                ClosingTime: s.ClosingTime,
                ApplicableDays: s.ApplicableDays,
                Priority: s.Priority,
                IsActive: s.IsActive,
                CreatedAtUtc: s.CreatedAtUtc,
                UpdatedAtUtc: s.UpdatedAtUtc
            ))
            .ToList();

        var closures = site.Closures
            .OrderBy(c => c.StartDate)
            .ThenBy(c => c.CreatedAtUtc)
            .Select(c => new SiteClosureDto(
                ClosureId: c.Id,
                Type: c.Type.ToString(),
                Reason: c.Reason.ToString(),
                Description: c.Description,
                StartDate: c.StartDate,
                EndDate: c.EndDate,
                ModifiedOpeningTime: c.ModifiedOpeningTime,
                ModifiedClosingTime: c.ModifiedClosingTime,
                AffectedCourtIds: c.AffectedCourtIds,
                CreatedAtUtc: c.CreatedAtUtc,
                UpdatedAtUtc: c.UpdatedAtUtc
            ))
            .ToList();

        var result = new SiteScheduleDetailDto(
            SiteId: site.Id,
            SiteName: site.Name,
            Timezone: site.Timezone,
            Schedules: schedules,
            Closures: closures
        );

        return Result.Success(result);
    }
}