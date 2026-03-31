// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Common;

namespace PadTime.Domain.Booking.Events;

/// <summary>
/// Raised when a match is cancelled by an administrator before completion.
/// </summary>
public sealed record MatchCancelledEvent(
    Guid MatchId,
    DateTime OccurredOnUtc) : IDomainEvent;