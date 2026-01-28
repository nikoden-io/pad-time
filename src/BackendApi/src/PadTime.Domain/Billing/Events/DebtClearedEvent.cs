using PadTime.Domain.Common;

namespace PadTime.Domain.Billing.Events;

public sealed record DebtClearedEvent(
    Guid DebtId,
    Guid MemberId,
    DateTime OccurredOnUtc) : IDomainEvent;
