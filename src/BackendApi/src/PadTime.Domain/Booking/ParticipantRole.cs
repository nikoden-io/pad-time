namespace PadTime.Domain.Booking;

/// <summary>
/// Role of a participant in a match.
/// </summary>
public enum ParticipantRole
{
    /// <summary>
    /// The member who created the match.
    /// Responsible for filling the match and paying for unfilled spots.
    /// </summary>
    Organizer = 1,

    /// <summary>
    /// A regular player in the match.
    /// </summary>
    Player = 2
}
