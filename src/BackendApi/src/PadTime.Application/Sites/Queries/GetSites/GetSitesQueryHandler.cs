// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Application.Common.Models;
using PadTime.Application.Sites.Queries.GetCourts;

namespace PadTime.Application.Sites.Queries.GetSites;

/// <summary>
/// Handler for GetSitesQuery.
/// Retrieves sites with pagination, search, and filtering support.
/// </summary>
public sealed class GetSitesQueryHandler(ISiteRepository siteRepository) 
    : IRequestHandler<GetSitesQuery, PagedResult<SiteDto>>
{
    public async Task<PagedResult<SiteDto>> Handle(
        GetSitesQuery request,
        CancellationToken cancellationToken)
    {
        var pagedSites = await siteRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.IsActive,
            request.City,
            request.Country,
            cancellationToken);

        var siteDtos = pagedSites.Items
            .Select(s => new SiteDto(
                SiteId: s.Id,
                Name: s.Name,
                StreetNumber: s.StreetNumber,
                Street: s.Street,
                Postcode: s.Postcode,
                City: s.City,
                Country: s.Country,
                Timezone: s.Timezone,
                IsActive: s.IsActive,
                CreatedAtUtc: s.CreatedAtUtc,
                CourtCount: s.Courts.Count,
                Courts: s.Courts
                    .OrderBy(c => c.Label)
                    .Select(c => new CourtDto(c.Id, c.Label, c.IsActive))
                    .ToList()))
            .ToList();

        return new PagedResult<SiteDto>(
            siteDtos,
            pagedSites.Page,
            pagedSites.PageSize,
            pagedSites.TotalCount);
    }
}