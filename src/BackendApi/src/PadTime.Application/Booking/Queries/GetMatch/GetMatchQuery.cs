using MediatR;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;

namespace PadTime.Application.Booking.Queries.GetMatch;

public sealed record GetMatchQuery(Guid MatchId) : IRequest<Result<MatchDto>>;

public sealed record MatchDto(
    Guid MatchId,
    Guid SiteId,
    Guid CourtId,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    string Type,
    string Status,
    Guid OrganizerId,
    int PriceTotalCents,
    IReadOnlyList<ParticipantDto> Participants);

public sealed record ParticipantDto(
    Guid MemberId,
    string Matricule,
    string Role,
    string PaymentStatus);
