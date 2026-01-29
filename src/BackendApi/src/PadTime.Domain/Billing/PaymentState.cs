namespace PadTime.Domain.Billing;

/// <summary>
/// State of a payment transaction.
/// </summary>
public enum PaymentState
{
    /// <summary>
    /// Payment initiated, awaiting processing.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Payment successfully processed.
    /// </summary>
    Paid = 2,

    /// <summary>
    /// Payment processing failed.
    /// </summary>
    Failed = 3
}
