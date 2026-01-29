using PadTime.Domain.Common;

namespace PadTime.Domain.Billing.Events;

public sealed record PaymentCreatedEvent(
    Guid PaymentId,
    Guid MatchId,
    Guid MemberId,
    int AmountCents,
    PaymentPurpose Purpose,
    DateTime OccurredOnUtc) : IDomainEvent;
