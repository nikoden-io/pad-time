// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Common;

namespace PadTime.Domain.Billing.Events;

/// <summary>
/// Raised when payment processing fails. The participant's slot may be released.
/// </summary>
public sealed record PaymentFailedEvent(
    Guid PaymentId,
    Guid MatchId,
    Guid MemberId,
    Guid ParticipantId,
    DateTime OccurredOnUtc) : IDomainEvent;