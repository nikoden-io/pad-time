using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Booking.Queries.GetPublicMatches;

public sealed record GetPublicMatchesQuery(
    Guid? SiteId,
    DateTime? FromUtc,
    DateTime? ToUtc,
    int Page,
    int PageSize) : IRequest<Result<IReadOnlyList<PublicMatchDto>>>;

public sealed record PublicMatchDto(
    Guid MatchId,
    Guid SiteId,
    Guid CourtId,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    string Status,
    Guid OrganizerId,
    int PriceTotalCents,
    int ParticipantCount,
    int AvailableSeats,
    IReadOnlyList<ParticipantSummaryDto> Participants);

public sealed record ParticipantSummaryDto(
    Guid MemberId,
    string Matricule,
    string Role,
    string PaymentStatus);
