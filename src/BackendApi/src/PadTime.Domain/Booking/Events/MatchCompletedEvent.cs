// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Common;

namespace PadTime.Domain.Booking.Events;

/// <summary>
/// Raised when a match reaches its end time and transitions to the Completed state.
/// </summary>
public sealed record MatchCompletedEvent(
    Guid MatchId,
    DateTime OccurredOnUtc) : IDomainEvent;