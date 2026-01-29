using PadTime.Domain.Common;

namespace PadTime.Domain.Booking;

/// <summary>
/// Defines the operating hours and slot configuration for a site in a specific year.
/// </summary>
public sealed class SiteYearSchedule : Entity<Guid>
{
    /// <summary>
    /// Standard slot duration in minutes.
    /// </summary>
    public const int SlotDurationMinutes = 90;

    /// <summary>
    /// Standard break between slots in minutes.
    /// </summary>
    public const int BreakDurationMinutes = 15;

    public Guid SiteId { get; private set; }

    /// <summary>
    /// Calendar year this schedule applies to.
    /// </summary>
    public int Year { get; private set; }

    /// <summary>
    /// Daily opening time (local time).
    /// </summary>
    public TimeOnly OpeningTime { get; private set; }

    /// <summary>
    /// Daily closing time (local time).
    /// </summary>
    public TimeOnly ClosingTime { get; private set; }

    private SiteYearSchedule() { } // EF Core

    public static SiteYearSchedule Create(
        Guid siteId,
        int year,
        TimeOnly openingTime,
        TimeOnly closingTime)
    {
        if (closingTime <= openingTime)
            throw new ArgumentException("Closing time must be after opening time.");

        return new SiteYearSchedule
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            Year = year,
            OpeningTime = openingTime,
            ClosingTime = closingTime
        };
    }

    /// <summary>
    /// Generates all available time slots for a given date.
    /// </summary>
    public IEnumerable<TimeSlot> GenerateSlots(DateOnly date)
    {
        var slotStart = OpeningTime;
        var totalSlotDuration = TimeSpan.FromMinutes(SlotDurationMinutes + BreakDurationMinutes);

        while (true)
        {
            var slotEnd = slotStart.Add(TimeSpan.FromMinutes(SlotDurationMinutes));

            // Check if slot fits within operating hours
            if (slotEnd > ClosingTime)
                break;

            yield return new TimeSlot(date, slotStart, slotEnd);

            // Move to next slot (including break)
            slotStart = slotStart.Add(totalSlotDuration);
        }
    }
}
