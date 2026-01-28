using PadTime.Domain.Common;

namespace PadTime.Domain.Booking.Events;

public sealed record MatchCreatedEvent(
    Guid MatchId,
    Guid SiteId,
    Guid CourtId,
    DateTime StartAtUtc,
    PadMatchType Type,
    DateTime OccurredOnUtc) : IDomainEvent;
