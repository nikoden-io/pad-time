using PadTime.Domain.Common;

namespace PadTime.Domain.Booking.Events;

public sealed record ParticipantExcludedEvent(
    Guid MatchId,
    Guid MemberId,
    DateTime OccurredOnUtc) : IDomainEvent;
