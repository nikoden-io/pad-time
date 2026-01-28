using PadTime.Domain.Common;

namespace PadTime.Domain.Booking.Events;

public sealed record ParticipantPaidEvent(
    Guid MatchId,
    Guid MemberId,
    DateTime OccurredOnUtc) : IDomainEvent;
