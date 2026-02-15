using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Queries.GetSiteById;

/// <summary>
/// Query to get detailed site information by ID.
/// </summary>
public sealed record GetSiteByIdQuery(Guid SiteId) : IRequest<Result<SiteDetailDto>>;

/// <summary>
/// DTO representing detailed site information.
/// </summary>
public sealed record SiteDetailDto(
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
    DateTime? UpdatedAtUtc,
    IReadOnlyList<CourtDetailDto> Courts,
    IReadOnlyList<SiteScheduleDto> Schedules,
    IReadOnlyList<SiteClosureDto> Closures
);
