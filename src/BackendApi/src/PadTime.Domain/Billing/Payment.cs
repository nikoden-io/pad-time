// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Billing.Events;
using PadTime.Domain.Common;

namespace PadTime.Domain.Billing;

/// <summary>
/// Aggregate root representing a payment transaction.
/// Follows a simple state machine: Pending -> Paid or Failed.
/// Uses an idempotency key to prevent duplicate processing.
/// </summary>
public sealed class Payment : AggregateRoot<Guid>
{
    /// <summary>
    /// The match this payment is associated with.
    /// </summary>
    public Guid MatchId { get; private set; }

    /// <summary>
    /// The member who initiated the payment.
    /// </summary>
    public Guid MemberId { get; private set; }

    /// <summary>
    /// The participant entry this payment covers.
    /// </summary>
    public Guid ParticipantId { get; private set; }

    /// <summary>
    /// Payment amount in cents.
    /// </summary>
    public int AmountCents { get; private set; }

    /// <summary>
    /// The business purpose of this payment (e.g., match participation or debt settlement).
    /// </summary>
    public PaymentPurpose Purpose { get; private set; }

    /// <summary>
    /// Current processing state of the payment.
    /// </summary>
    public PaymentState State { get; private set; }

    /// <summary>
    /// Client-provided idempotency key to prevent duplicate payments.
    /// </summary>
    public string IdempotencyKey { get; private set; } = null!;

    /// <summary>
    /// When the payment was created (UTC).
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// When the payment was processed (UTC). Null while pending.
    /// </summary>
    public DateTime? ProcessedAtUtc { get; private set; }

    private Payment() { } // EF Core

    /// <summary>
    /// Creates a new payment in the <see cref="PaymentState.Pending"/> state.
    /// Raises <see cref="Events.PaymentCreatedEvent"/> on success.
    /// </summary>
    /// <param name="matchId">The match being paid for.</param>
    /// <param name="memberId">The member making the payment.</param>
    /// <param name="participantId">The participant entry this payment covers.</param>
    /// <param name="amountCents">Amount in cents (must be positive).</param>
    /// <param name="purpose">The business purpose of the payment.</param>
    /// <param name="idempotencyKey">Unique key to prevent duplicate payments.</param>
    /// <param name="utcNow">Current UTC timestamp.</param>
    /// <returns>A result containing the created payment or a domain error.</returns>
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