// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Common;

namespace PadTime.Domain.Booking;

/// <summary>
/// Value object representing a bookable time slot.
/// </summary>
public sealed class TimeSlot : ValueObject
{
    /// <summary>
    /// The date of the time slot.
    /// </summary>
    public DateOnly Date { get; }

    /// <summary>
    /// The slot start time (local time).
    /// </summary>
    public TimeOnly StartTime { get; }

    /// <summary>
    /// The slot end time (local time).
    /// </summary>
    public TimeOnly EndTime { get; }

    /// <summary>
    /// Creates a new time slot for the specified date and time range.
    /// </summary>
    /// <param name="date">The date of the slot.</param>
    /// <param name="startTime">The start time (must be before <paramref name="endTime"/>).</param>
    /// <param name="endTime">The end time.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="endTime"/> is not after <paramref name="startTime"/>.</exception>
    public TimeSlot(DateOnly date, TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime <= startTime)
            throw new ArgumentException("End time must be after start time.");

        Date = date;
        StartTime = startTime;
        EndTime = endTime;
    }

    /// <summary>
    /// Creates a <see cref="TimeSlot"/> from two <see cref="DateTime"/> values, extracting date and time components.
    /// </summary>
    /// <param name="start">The start date and time.</param>
    /// <param name="end">The end date and time (must be after <paramref name="start"/>).</param>
    public static TimeSlot FromDateTimes(DateTime start, DateTime end)
    {
        if (end <= start)
            throw new ArgumentException("End time must be after start time.");

        return new TimeSlot(
            DateOnly.FromDateTime(start),
            TimeOnly.FromDateTime(start),
            TimeOnly.FromDateTime(end));
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

    /// <summary>
    /// The duration of the time slot.
    /// </summary>
    public TimeSpan Duration => EndTime - StartTime;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Date;
        yield return StartTime;
        yield return EndTime;
    }

    public override string ToString() => $"{Date:yyyy-MM-dd} {StartTime:HH:mm}-{EndTime:HH:mm}";
}