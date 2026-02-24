using PadTime.Domain.Booking;
using PadTime.Domain.Common;

namespace PadTime.Domain.Site;

/// <summary>
/// Domain service for managing site schedules and availability.
/// Resolves conflicts between schedules, closures, and exceptions.
/// </summary>
public sealed class SiteAvailabilityService
{
    public static SiteSchedule? GetApplicableSchedule(
        IEnumerable<SiteSchedule> schedules,
        DateOnly date)
    {
        return schedules
            .Where(s => s.IsApplicableOn(date))
            .OrderByDescending(s => s.Priority)
            .FirstOrDefault();
    }

    public static bool IsSiteClosed(
        IEnumerable<SiteClosure> closures,
        DateOnly date)
    {
        foreach (var closure in closures)
        {
            if (closure.IsFullyClosed(date))
                return true;
        }
        return false;
    }

    public static bool IsCourtClosed(
        IEnumerable<SiteClosure> closures,
        DateOnly date,
        Guid courtId)
    {
        foreach (var closure in closures)
        {
            if (closure.IsFullyClosed(date, courtId))
                return true;
        }
        return false;
    }

    public static (TimeOnly opening, TimeOnly closing)? GetEffectiveHours(
        IEnumerable<SiteSchedule> schedules,
        IEnumerable<SiteClosure> closures,
        DateOnly date)
    {
        if (IsSiteClosed(closures, date))
            return null;

        var reducedHours = closures
            .Select(c => c.GetModifiedHours(date))
            .FirstOrDefault(h => h.HasValue);

        if (reducedHours.HasValue)
            return reducedHours.Value;

        var schedule = GetApplicableSchedule(schedules, date);
        if (schedule == null)
            return null;

        return (schedule.OpeningTime, schedule.ClosingTime);
    }

    public static IEnumerable<TimeSlot> GetAvailableSlots(
        IEnumerable<SiteSchedule> schedules,
        IEnumerable<SiteClosure> closures,
        DateOnly date,
        Guid? courtId = null)
    {
        if (courtId.HasValue)
        {
            if (IsCourtClosed(closures, date, courtId.Value))
                return Enumerable.Empty<TimeSlot>();
        }
        else
        {
            if (IsSiteClosed(closures, date))
                return Enumerable.Empty<TimeSlot>();
        }

        var effectiveHours = GetEffectiveHours(schedules, closures, date);
        if (!effectiveHours.HasValue)
            return Enumerable.Empty<TimeSlot>();

        var (opening, closing) = effectiveHours.Value;
        return GenerateSlots(date, opening, closing);
    }

    private static IEnumerable<TimeSlot> GenerateSlots(
        DateOnly date,
        TimeOnly openingTime,
        TimeOnly closingTime)
    {
        var slotStart = openingTime;
        var totalSlotDuration = TimeSpan.FromMinutes(
            SiteSchedule.SlotDurationMinutes + SiteSchedule.BreakDurationMinutes);

        while (true)
        {
            var slotEnd = slotStart.Add(TimeSpan.FromMinutes(SiteSchedule.SlotDurationMinutes));

            if (slotEnd > closingTime)
                break;

            yield return new TimeSlot(date, slotStart, slotEnd);

            slotStart = slotStart.Add(totalSlotDuration);
        }
    }

    public static Result ValidateScheduleOverlap(
        SiteSchedule newSchedule,
        IEnumerable<SiteSchedule> existingSchedules)
    {
        var conflicts = existingSchedules
            .Where(s => s.Id != newSchedule.Id && s.IsActive)
            .Where(s => SchedulesOverlap(newSchedule, s))
            .ToList();

        if (conflicts.Count == 0)
            return Result.Success();

        if (conflicts.All(c => c.Priority != newSchedule.Priority))
            return Result.Success();

        if (conflicts.All(c => !HasOverlappingDays(newSchedule, c)))
            return Result.Success();

        return DomainErrors.Site.ScheduleConflict;
    }

    private static bool SchedulesOverlap(SiteSchedule s1, SiteSchedule s2)
    {
        if (s1.ValidUntil.HasValue && s2.ValidFrom > s1.ValidUntil.Value)
            return false;

        if (s2.ValidUntil.HasValue && s1.ValidFrom > s2.ValidUntil.Value)
            return false;

        return true;
    }

    private static bool HasOverlappingDays(SiteSchedule s1, SiteSchedule s2)
    {
        if (s1.ApplicableDays == null || s2.ApplicableDays == null)
            return true;

        return s1.ApplicableDays.Intersect(s2.ApplicableDays).Any();
    }
}
