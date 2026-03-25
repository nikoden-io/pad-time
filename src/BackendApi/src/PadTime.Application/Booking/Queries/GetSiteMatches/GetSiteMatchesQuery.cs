using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Booking.Queries.GetSiteMatches;

public sealed record GetSiteMatchesQuery(
    Guid SiteId,
    DateTime? FromUtc,
    DateTime? ToUtc,
    int Page,
    int PageSize) : IRequest<Result<IReadOnlyList<SiteMatchDto>>>;

public sealed record SiteMatchDto(
    Guid MatchId,
    Guid SiteId,
    Guid CourtId,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    string Type,
    string Status,
    Guid OrganizerId,
    int PriceTotalCents,
    int ParticipantCount);
