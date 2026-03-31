// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Common;

namespace PadTime.Domain.Booking.Events;

/// <summary>
/// Raised when a new match is created by an organizer, including court and time slot assignment.
/// </summary>
public sealed record MatchCreatedEvent(
    Guid MatchId,
    Guid SiteId,
    Guid CourtId,
    DateTime StartAtUtc,
    PadMatchType Type,
    DateTime OccurredOnUtc) : IDomainEvent;