using PadTime.Domain.Common;

namespace PadTime.Domain.Billing.Events;

public sealed record DebtCreatedEvent(
    Guid DebtId,
    Guid MemberId,
    int AmountCents,
    DateTime OccurredOnUtc) : IDomainEvent;
