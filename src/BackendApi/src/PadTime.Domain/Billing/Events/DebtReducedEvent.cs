// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Common;

namespace PadTime.Domain.Billing.Events;

/// <summary>
/// Raised when a payment reduces an organizer's outstanding debt.
/// </summary>
public sealed record DebtReducedEvent(
    Guid DebtId,
    Guid MemberId,
    int PaymentCents,
    int NewTotalCents,
    DateTime OccurredOnUtc) : IDomainEvent;