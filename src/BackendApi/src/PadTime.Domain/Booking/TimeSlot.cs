using PadTime.Domain.Common;

namespace PadTime.Domain.Booking;

/// <summary>
/// Value object representing a bookable time slot.
/// </summary>
public sealed class TimeSlot : ValueObject
{
    public DateOnly Date { get; }
    public TimeOnly StartTime { get; }
    public TimeOnly EndTime { get; }

    public TimeSlot(DateOnly date, TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime <= startTime)
            throw new ArgumentException("End time must be after start time.");

        Date = date;
        StartTime = startTime;
        EndTime = endTime;
    }

    /// <summary>
    /// Converts the slot start to a UTC DateTime using the specified timezone.
    /// </summary>
    public DateTime ToUtcStart(TimeZoneInfo timezone)
    {
        var localDateTime = Date.ToDateTime(StartTime);
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, timezone);
    }

    /// <summary>
    /// Converts the slot end to a UTC DateTime using the specified timezone.
    /// </summary>
    public DateTime ToUtcEnd(TimeZoneInfo timezone)
    {
        var localDateTime = Date.ToDateTime(EndTime);
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, timezone);
    }

    public TimeSpan Duration => EndTime - StartTime;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Date;
        yield return StartTime;
        yield return EndTime;
    }

    public override string ToString() => $"{Date:yyyy-MM-dd} {StartTime:HH:mm}-{EndTime:HH:mm}";
}
