using MediatR;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Booking;

namespace PadTime.Application.Sites.Queries.GetSites;

/// <summary>
/// Handler for GetSitesQuery.
/// Retrieves all active sites from the repository and maps them to DTOs.
/// </summary>
public sealed class GetSitesQueryHandler(ISiteRepository siteRepository) : IRequestHandler<GetSitesQuery, List<SiteDto>>
{
    public async Task<List<SiteDto>> Handle(
        GetSitesQuery request,
        CancellationToken cancellationToken)
    {
        List<Site> sites = await siteRepository.GetAllActiveAsync(cancellationToken);

        var siteDtos = sites
            .Select(s => new SiteDto(
                SiteId: s.Id,
                Name: s.Name,
                Timezone: s.Timezone))
            .ToList();

        return siteDtos;
    }
}
