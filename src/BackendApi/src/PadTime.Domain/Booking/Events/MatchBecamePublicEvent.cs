using PadTime.Domain.Common;

namespace PadTime.Domain.Booking.Events;

public sealed record MatchBecamePublicEvent(
    Guid MatchId,
    DateTime OccurredOnUtc) : IDomainEvent;
