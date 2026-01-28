using PadTime.Domain.Billing.Events;
using PadTime.Domain.Common;

namespace PadTime.Domain.Billing;

/// <summary>
/// Aggregate root for a payment transaction (mock).
/// </summary>
public sealed class Payment : AggregateRoot<Guid>
{
    public Guid MatchId { get; private set; }
    public Guid MemberId { get; private set; }
    public Guid ParticipantId { get; private set; }

    /// <summary>
    /// Payment amount in cents.
    /// </summary>
    public int AmountCents { get; private set; }

    public PaymentPurpose Purpose { get; private set; }
    public PaymentState State { get; private set; }

    /// <summary>
    /// Client-provided idempotency key to prevent duplicate payments.
    /// </summary>
    public string IdempotencyKey { get; private set; } = null!;

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ProcessedAtUtc { get; private set; }

    private Payment() { } // EF Core

    public static Result<Payment> Create(
        Guid matchId,
        Guid memberId,
        Guid participantId,
        int amountCents,
        PaymentPurpose purpose,
        string idempotencyKey,
        DateTime utcNow)
    {
        if (amountCents <= 0)
            return DomainErrors.Billing.InvalidAmount;

        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return DomainErrors.Billing.IdempotencyConflict;

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            MatchId = matchId,
            MemberId = memberId,
            ParticipantId = participantId,
            AmountCents = amountCents,
            Purpose = purpose,
            State = PaymentState.Pending,
            IdempotencyKey = idempotencyKey,
            CreatedAtUtc = utcNow
        };

        payment.RaiseDomainEvent(new PaymentCreatedEvent(
            payment.Id, matchId, memberId, amountCents, purpose, utcNow));

        return payment;
    }

    /// <summary>
    /// Simulates successful payment processing.
    /// </summary>
    public Result MarkAsPaid(DateTime utcNow)
    {
        if (State != PaymentState.Pending)
            return DomainErrors.Billing.PaymentAlreadyProcessed;

        State = PaymentState.Paid;
        ProcessedAtUtc = utcNow;

        RaiseDomainEvent(new PaymentSucceededEvent(Id, MatchId, MemberId, ParticipantId, AmountCents, utcNow));

        return Result.Success();
    }

    /// <summary>
    /// Simulates failed payment processing.
    /// </summary>
    public Result MarkAsFailed(DateTime utcNow)
    {
        if (State != PaymentState.Pending)
            return DomainErrors.Billing.PaymentAlreadyProcessed;

        State = PaymentState.Failed;
        ProcessedAtUtc = utcNow;

        RaiseDomainEvent(new PaymentFailedEvent(Id, MatchId, MemberId, ParticipantId, utcNow));

        return Result.Success();
    }
}
