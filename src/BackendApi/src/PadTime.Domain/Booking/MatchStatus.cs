namespace PadTime.Domain.Booking;

/// <summary>
/// State machine states for a match.
/// Transitions are enforced by the Match aggregate.
/// </summary>
public enum MatchStatus
{
    /// <summary>
    /// Initial state (not used in practice, transitions immediately).
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Private match - participants added by organizer.
    /// </summary>
    Private = 1,

    /// <summary>
    /// Public match - open for anyone to join (first paid = first served).
    /// </summary>
    Public = 2,

    /// <summary>
    /// Match is full (4 paid participants).
    /// </summary>
    Full = 3,

    /// <summary>
    /// Match start time reached - no more changes allowed.
    /// </summary>
    Locked = 4,

    /// <summary>
    /// Match end time reached - completed successfully.
    /// </summary>
    Completed = 5,

    /// <summary>
    /// Match cancelled by admin.
    /// </summary>
    Cancelled = 6
}
