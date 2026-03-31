// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Common;

namespace PadTime.Domain.Billing.Events;

/// <summary>
/// Raised when payment processing succeeds. Triggers participant confirmation on the match.
/// </summary>
public sealed record PaymentSucceededEvent(
    Guid PaymentId,
    Guid MatchId,
    Guid MemberId,
    Guid ParticipantId,
    int AmountCents,
    DateTime OccurredOnUtc) : IDomainEvent;