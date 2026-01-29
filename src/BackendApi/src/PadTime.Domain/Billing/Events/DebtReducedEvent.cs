using PadTime.Domain.Common;

namespace PadTime.Domain.Billing.Events;

public sealed record DebtReducedEvent(
    Guid DebtId,
    Guid MemberId,
    int PaymentCents,
    int NewTotalCents,
    DateTime OccurredOnUtc) : IDomainEvent;
