using PadTime.Domain.Billing.Events;
using PadTime.Domain.Common;

namespace PadTime.Domain.Billing;

/// <summary>
/// Aggregate root tracking an organizer's outstanding debt.
/// Debt blocks match creation until cleared.
/// </summary>
public sealed class OrganizerDebt : AggregateRoot<Guid>
{
    public Guid MemberId { get; private set; }

    /// <summary>
    /// Outstanding debt amount in cents.
    /// </summary>
    public int AmountCents { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private OrganizerDebt() { } // EF Core

    public static OrganizerDebt Create(Guid memberId, int initialAmountCents, DateTime utcNow)
    {
        if (initialAmountCents < 0)
            throw new ArgumentException("Debt amount cannot be negative.", nameof(initialAmountCents));

        var debt = new OrganizerDebt
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            AmountCents = initialAmountCents,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow
        };

        if (initialAmountCents > 0)
        {
            debt.RaiseDomainEvent(new DebtCreatedEvent(debt.Id, memberId, initialAmountCents, utcNow));
        }

        return debt;
    }

    /// <summary>
    /// Increases the debt (e.g., incomplete match penalty).
    /// </summary>
    public void IncreaseDebt(int amountCents, DateTime utcNow)
    {
        if (amountCents <= 0)
            throw new ArgumentException("Amount must be positive.", nameof(amountCents));

        AmountCents += amountCents;
        UpdatedAtUtc = utcNow;

        RaiseDomainEvent(new DebtIncreasedEvent(Id, MemberId, amountCents, AmountCents, utcNow));
    }

    /// <summary>
    /// Reduces the debt by a payment amount.
    /// </summary>
    public void ApplyPayment(int paymentCents, DateTime utcNow)
    {
        if (paymentCents <= 0)
            throw new ArgumentException("Payment must be positive.", nameof(paymentCents));

        var previousAmount = AmountCents;
        AmountCents = Math.Max(0, AmountCents - paymentCents);
        UpdatedAtUtc = utcNow;

        RaiseDomainEvent(new DebtReducedEvent(Id, MemberId, paymentCents, AmountCents, utcNow));

        if (previousAmount > 0 && AmountCents == 0)
        {
            RaiseDomainEvent(new DebtClearedEvent(Id, MemberId, utcNow));
        }
    }

    /// <summary>
    /// Whether the organizer has outstanding debt.
    /// </summary>
    public bool HasDebt => AmountCents > 0;

    /// <summary>
    /// Whether the organizer can create new matches.
    /// </summary>
    public bool CanCreateMatch => !HasDebt;
}
