using PadTime.Domain.Common;

namespace PadTime.Domain.Booking.Events;

/// <summary>
/// Raised when a match is locked with fewer than 4 paid participants.
/// The organizer will be charged for the missing spots.
/// </summary>
public sealed record MatchIncompleteEvent(
    Guid MatchId,
    Guid OrganizerId,
    int DebtAmountCents,
    DateTime OccurredOnUtc) : IDomainEvent;
