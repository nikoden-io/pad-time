namespace PadTime.Domain.Common;

/// <summary>
/// Abstraction for date/time operations to enable testing.
/// All times are in UTC.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Current UTC date (time component is 00:00:00).
    /// </summary>
    DateOnly Today { get; }
}
