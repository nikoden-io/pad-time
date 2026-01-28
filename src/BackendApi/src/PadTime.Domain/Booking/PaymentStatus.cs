namespace PadTime.Domain.Booking;

/// <summary>
/// Payment status for a participant.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment not yet initiated.
    /// </summary>
    Unpaid = 0,

    /// <summary>
    /// Payment initiated, waiting for confirmation.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Payment confirmed.
    /// </summary>
    Paid = 2,

    /// <summary>
    /// Payment failed.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Participant excluded (unpaid at J-1 deadline).
    /// </summary>
    Excluded = 4
}
