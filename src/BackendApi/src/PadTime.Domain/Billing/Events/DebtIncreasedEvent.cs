// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Common;

namespace PadTime.Domain.Billing.Events;

/// <summary>
/// Raised when an organizer's existing debt increases, for example due to another incomplete match penalty.
/// </summary>
public sealed record DebtIncreasedEvent(
    Guid DebtId,
    Guid MemberId,
    int IncreaseCents,
    int NewTotalCents,
    DateTime OccurredOnUtc) : IDomainEvent;