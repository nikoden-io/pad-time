namespace PadTime.Domain.Billing;

/// <summary>
/// Purpose of a payment transaction.
/// </summary>
public enum PaymentPurpose
{
    /// <summary>
    /// Payment for a match participation slot.
    /// </summary>
    MatchParticipation = 1,

    /// <summary>
    /// Payment to clear organizer debt.
    /// </summary>
    DebtSettlement = 2
}
