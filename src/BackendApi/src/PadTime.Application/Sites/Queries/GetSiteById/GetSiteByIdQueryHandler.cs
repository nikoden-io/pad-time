using MediatR;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Queries.GetSiteById;

/// <summary>
/// Handler for GetSiteByIdQuery.
/// Retrieves detailed site information including courts, schedules, and closures.
/// </summary>
public sealed class GetSiteByIdQueryHandler(ISiteRepository siteRepository)
    : IRequestHandler<GetSiteByIdQuery, Result<SiteDetailDto>>
{
    public async Task<Result<SiteDetailDto>> Handle(
        GetSiteByIdQuery request,
        CancellationToken cancellationToken)
    {
        var site = await siteRepository.GetByIdWithSchedulesAndClosuresAsync(
            request.SiteId,
            cancellationToken);

        if (site is null)
            return DomainErrors.Site.NotFound;

        var siteDetailDto = new SiteDetailDto(
            SiteId: site.Id,
            Name: site.Name,
            StreetNumber: site.StreetNumber,
            Street: site.Street,
            Postcode: site.Postcode,
            City: site.City,
            Country: site.Country,
            Timezone: site.Timezone,
            IsActive: site.IsActive,
            CreatedAtUtc: site.CreatedAtUtc,
            UpdatedAtUtc: site.UpdatedAtUtc,
            Courts: site.Courts
                .OrderBy(c => c.Label)
                .Select(c => new CourtDetailDto(
                    CourtId: c.Id,
                    Label: c.Label,
                    IsActive: c.IsActive,
                    CreatedAtUtc: c.CreatedAtUtc))
                .ToList(),
            Schedules: site.Schedules
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
                    UpdatedAtUtc: s.UpdatedAtUtc))
                .ToList(),
            Closures: site.Closures
                .OrderByDescending(c => c.CreatedAtUtc)
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
                    UpdatedAtUtc: c.UpdatedAtUtc))
                .ToList()
        );

        return siteDetailDto;
    }
}
