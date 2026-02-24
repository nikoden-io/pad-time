using PadTime.Domain.Common;

namespace PadTime.Domain.Billing.Events;

public sealed record PaymentSucceededEvent(
    Guid PaymentId,
    Guid MatchId,
    Guid MemberId,
    Guid ParticipantId,
    int AmountCents,
    DateTime OccurredOnUtc) : IDomainEvent;
