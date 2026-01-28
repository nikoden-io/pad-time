using PadTime.Domain.Common;

namespace PadTime.Domain.Booking.Events;

public sealed record MatchCompletedEvent(
    Guid MatchId,
    DateTime OccurredOnUtc) : IDomainEvent;
