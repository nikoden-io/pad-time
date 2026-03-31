// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Common;

namespace PadTime.Domain.Booking.Events;

/// <summary>
/// Raised when a participant's payment is confirmed for a match.
/// </summary>
public sealed record ParticipantPaidEvent(
    Guid MatchId,
    Guid MemberId,
    DateTime OccurredOnUtc) : IDomainEvent;