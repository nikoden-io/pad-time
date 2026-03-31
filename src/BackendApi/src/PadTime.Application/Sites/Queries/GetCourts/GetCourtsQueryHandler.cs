// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Queries.GetCourts;

public sealed class GetCourtsQueryHandler(ISiteRepository siteRepository)
    : IRequestHandler<GetCourtsQuery, Result<List<CourtDto>>>
{
    public async Task<Result<List<CourtDto>>> Handle(
        GetCourtsQuery request,
        CancellationToken cancellationToken)
    {
        var site = await siteRepository.GetByIdAsync(request.SiteId, cancellationToken);
        if (site is null)
            return DomainErrors.Site.NotFound;

        var courts = site.Courts
            .OrderBy(c => c.Label)
            .Select(c => new CourtDto(
                CourtId: c.Id,
                Label: c.Label,
                IsActive: c.IsActive))
            .ToList();

        return courts;
    }
}