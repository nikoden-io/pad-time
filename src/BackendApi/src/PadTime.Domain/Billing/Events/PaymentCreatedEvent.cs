// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Common;

namespace PadTime.Domain.Billing.Events;

/// <summary>
/// Raised when a new payment is initiated for a match participation or debt settlement.
/// </summary>
public sealed record PaymentCreatedEvent(
    Guid PaymentId,
    Guid MatchId,
    Guid MemberId,
    int AmountCents,
    PaymentPurpose Purpose,
    DateTime OccurredOnUtc) : IDomainEvent;