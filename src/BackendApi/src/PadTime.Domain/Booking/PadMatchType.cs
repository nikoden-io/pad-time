namespace PadTime.Domain.Booking;

/// <summary>
/// Type of match determining join rules.
/// </summary>
public enum PadMatchType
{
    /// <summary>
    /// Private match - participants added manually by organizer.
    /// </summary>
    Private = 1,

    /// <summary>
    /// Public match - anyone can join by paying.
    /// </summary>
    Public = 2
}
