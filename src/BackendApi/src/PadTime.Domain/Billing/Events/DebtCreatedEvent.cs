// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Common;

namespace PadTime.Domain.Billing.Events;

/// <summary>
/// Raised when a new debt record is created for an organizer, typically due to an incomplete match.
/// </summary>
public sealed record DebtCreatedEvent(
    Guid DebtId,
    Guid MemberId,
    int AmountCents,
    DateTime OccurredOnUtc) : IDomainEvent;