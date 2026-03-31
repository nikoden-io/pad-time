// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Common;

namespace PadTime.Domain.Booking.Events;

/// <summary>
/// Raised when a participant is excluded from a match due to non-payment at the J-1 deadline.
/// </summary>
public sealed record ParticipantExcludedEvent(
    Guid MatchId,
    Guid MemberId,
    DateTime OccurredOnUtc) : IDomainEvent;