using MediatR;
using PadTime.Application.Common.Models;
using PadTime.Application.Sites.Queries.GetCourts;

namespace PadTime.Application.Sites.Queries.GetSites;

/// <summary>
/// Query to get sites with pagination, search, and filtering support.
/// </summary>
public sealed record GetSitesQuery(
    int Page = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    bool? IsActive = null,
    string? City = null,
    string? Country = null
) : IRequest<PagedResult<SiteDto>>;

/// <summary>
/// DTO representing a site in the response.
/// </summary>
public sealed record SiteDto(
    Guid SiteId,
    string Name,
    string StreetNumber,
    string Street,
    string Postcode,
    string City,
    string Country,
    string Timezone,
    bool IsActive,
    DateTime CreatedAtUtc,
    int CourtCount,
    IReadOnlyList<CourtDto> Courts
);
