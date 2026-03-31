// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Common;

namespace PadTime.Domain.Booking.Events;

/// <summary>
/// Raised when a private match transitions to public at the J-1 deadline because it has fewer than 4 paid participants.
/// </summary>
public sealed record MatchBecamePublicEvent(
    Guid MatchId,
    DateTime OccurredOnUtc) : IDomainEvent;