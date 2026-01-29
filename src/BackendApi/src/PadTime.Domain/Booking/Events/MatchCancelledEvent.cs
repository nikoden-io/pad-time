using PadTime.Domain.Common;

namespace PadTime.Domain.Booking.Events;

public sealed record MatchCancelledEvent(
    Guid MatchId,
    DateTime OccurredOnUtc) : IDomainEvent;
