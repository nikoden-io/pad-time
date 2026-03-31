// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Common;

namespace PadTime.Domain.Billing.Events;

/// <summary>
/// Raised when an organizer's debt is fully paid off, re-enabling match creation.
/// </summary>
public sealed record DebtClearedEvent(
    Guid DebtId,
    Guid MemberId,
    DateTime OccurredOnUtc) : IDomainEvent;