using PadTime.Domain.Common;

namespace PadTime.Domain.Billing.Events;

public sealed record DebtIncreasedEvent(
    Guid DebtId,
    Guid MemberId,
    int IncreaseCents,
    int NewTotalCents,
    DateTime OccurredOnUtc) : IDomainEvent;
