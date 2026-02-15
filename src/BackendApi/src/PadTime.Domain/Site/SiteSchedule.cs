using PadTime.Domain.Booking;
using PadTime.Domain.Common;

namespace PadTime.Domain.Site;

/// <summary>
/// Represents a site's operating schedule.
/// Can be a standard or seasonal schedule.
/// </summary>
public sealed class SiteSchedule : Entity<Guid>
{
    /// <summary>
    /// Standard slot duration in minutes (90 min).
    /// </summary>
    public const int SlotDurationMinutes = 90;

    /// <summary>
    /// Standard break duration between slots in minutes (15 min).
    /// </summary>
    public const int BreakDurationMinutes = 15;

    public Guid SiteId { get; private set; }

    /// <summary>
    /// Schedule name (e.g., "Summer Schedule", "Standard Schedule").
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Validity start date (inclusive).
    /// </summary>
    public DateOnly ValidFrom { get; private set; }

    /// <summary>
    /// Validity end date (inclusive).
    /// Null indicates a default schedule with no end date.
    /// </summary>
    public DateOnly? ValidUntil { get; private set; }

    /// <summary>
    /// Opening time (local time).
    /// </summary>
    public TimeOnly OpeningTime { get; private set; }

    /// <summary>
    /// Closing time (local time).
    /// </summary>
    public TimeOnly ClosingTime { get; private set; }

    /// <summary>
    /// Days of the week when this schedule applies.
    /// Null indicates all days.
    /// </summary>
    public DayOfWeek[]? ApplicableDays { get; private set; }

    /// <summary>
    /// Indicates whether this schedule is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Schedule priority (higher value = higher priority).
    /// Used to resolve conflicts between overlapping schedules.
    /// </summary>
    public int Priority { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private SiteSchedule() { } // EF Core

    public static Result<SiteSchedule> Create(
        Guid siteId,
        string name,
        DateOnly validFrom,
        DateOnly? validUntil,
        TimeOnly openingTime,
        TimeOnly closingTime,
        DayOfWeek[]? applicableDays,
        int priority,
        DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(name))
            return DomainErrors.Site.InvalidSchedule;

        if (closingTime <= openingTime)
            return DomainErrors.Site.InvalidSchedule;

        if (validUntil.HasValue && validUntil.Value < validFrom)
            return DomainErrors.Site.InvalidSchedule;

        if (applicableDays?.Length == 0)
            return DomainErrors.Site.InvalidSchedule;

        var schedule = new SiteSchedule
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            Name = name,
            ValidFrom = validFrom,
            ValidUntil = validUntil,
            OpeningTime = openingTime,
            ClosingTime = closingTime,
            ApplicableDays = applicableDays,
            IsActive = true,
            Priority = priority,
            CreatedAtUtc = utcNow
        };

        return schedule;
    }

    /// <summary>
    /// Updates the schedule properties.
    /// </summary>
    public Result Update(
        string name,
        DateOnly validFrom,
        DateOnly? validUntil,
        TimeOnly openingTime,
        TimeOnly closingTime,
        DayOfWeek[]? applicableDays,
        int priority,
        DateTime utcNow)
    {
        // Validations internes (comme dans Create)
        if (validUntil.HasValue && validUntil.Value < validFrom)
            return DomainErrors.SiteSchedule.InvalidDateRange;

        if (closingTime <= openingTime)
            return DomainErrors.SiteSchedule.InvalidTimeRange;

        // Mise à jour
        Name = name;
        ValidFrom = validFrom;
        ValidUntil = validUntil;
        OpeningTime = openingTime;
        ClosingTime = closingTime;
        ApplicableDays = applicableDays;
        Priority = priority;
        UpdatedAtUtc = utcNow;

        return Result.Success();
    }

    /// <summary>
    /// Checks if this schedule is applicable on a given date.
    /// </summary>
    public bool IsApplicableOn(DateOnly date)
    {
        if (!IsActive)
            return false;

        if (date < ValidFrom)
            return false;

        if (ValidUntil.HasValue && date > ValidUntil.Value)
            return false;

        if (ApplicableDays != null && ApplicableDays.Length > 0)
        {
            var dayOfWeek = date.DayOfWeek;
            if (!ApplicableDays.Contains(dayOfWeek))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Generates all available time slots for a given date.
    /// </summary>
    public IEnumerable<TimeSlot> GenerateSlots(DateOnly date)
    {
        if (!IsApplicableOn(date))
            yield break;

        var slotStart = OpeningTime;
        var totalSlotDuration = TimeSpan.FromMinutes(SlotDurationMinutes + BreakDurationMinutes);

        while (true)
        {
            var slotEnd = slotStart.Add(TimeSpan.FromMinutes(SlotDurationMinutes));

            if (slotEnd > ClosingTime)
                break;

            yield return new TimeSlot(date, slotStart, slotEnd);

            slotStart = slotStart.Add(totalSlotDuration);
        }
    }

    /// <summary>
    /// Updates the operating hours.
    /// </summary>
    public Result UpdateOperatingHours(TimeOnly openingTime, TimeOnly closingTime, DateTime utcNow)
    {
        if (closingTime <= openingTime)
            return DomainErrors.Site.InvalidSchedule;

        OpeningTime = openingTime;
        ClosingTime = closingTime;
        UpdatedAtUtc = utcNow;

        return Result.Success();
    }

    /// <summary>
    /// Extends the validity period.
    /// </summary>
    public Result ExtendValidity(DateOnly? newValidUntil, DateTime utcNow)
    {
        if (newValidUntil.HasValue && newValidUntil.Value < ValidFrom)
            return DomainErrors.Site.InvalidSchedule;

        ValidUntil = newValidUntil;
        UpdatedAtUtc = utcNow;

        return Result.Success();
    }

    public void Activate(DateTime utcNow)
    {
        IsActive = true;
        UpdatedAtUtc = utcNow;
    }

    public void Deactivate(DateTime utcNow)
    {
        IsActive = false;
        UpdatedAtUtc = utcNow;
    }
}
