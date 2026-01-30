using MediatR;

namespace PadTime.Application.Sites.Queries.GetSites;

/// <summary>
/// Query to get all active sites.
/// </summary>
public sealed record GetSitesQuery() : IRequest<List<SiteDto>>;

/// <summary>
/// DTO representing a site in the response.
/// </summary>
public sealed record SiteDto(
    Guid SiteId,
    string Name,
    string Timezone);
