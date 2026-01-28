using PadTime.Domain.Common;

namespace PadTime.Domain.Billing.Events;

public sealed record PaymentFailedEvent(
    Guid PaymentId,
    Guid MatchId,
    Guid MemberId,
    Guid ParticipantId,
    DateTime OccurredOnUtc) : IDomainEvent;
