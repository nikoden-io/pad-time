// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Booking;
using PadTime.Domain.Common;

namespace PadTime.Domain.Site;

/// <summary>
/// Domain service for managing site schedules and availability.
/// Resolves conflicts between schedules, closures, and exceptions.
/// </summary>
public sealed class SiteAvailabilityService
{
    /// <summary>
    /// Returns the highest-priority schedule applicable on the given date, or <c>null</c> if none applies.
    /// </summary>
    /// <param name="schedules">The site's configured schedules.</param>
    /// <param name="date">The date to check.</param>
    public static SiteSchedule? GetApplicableSchedule(
        IEnumerable<SiteSchedule> schedules,
        DateOnly date)
    {
        return schedules
            .Where(s => s.IsApplicableOn(date))
            .OrderByDescending(s => s.Priority)
            .FirstOrDefault();
    }

    /// <summary>
    /// Determines whether the entire site is fully closed on the given date.
    /// </summary>
    /// <param name="closures">The site's closures.</param>
    /// <param name="date">The date to check.</param>
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

    /// <summary>
    /// Determines whether a specific court is fully closed on the given date.
    /// </summary>
    /// <param name="closures">The site's closures.</param>
    /// <param name="date">The date to check.</param>
    /// <param name="courtId">The court to check.</param>
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

    /// <summary>
    /// Computes the effective operating hours for a date, accounting for reduced-hours closures.
    /// Returns <c>null</c> if the site is fully closed or has no applicable schedule.
    /// </summary>
    /// <param name="schedules">The site's configured schedules.</param>
    /// <param name="closures">The site's closures.</param>
    /// <param name="date">The date to check.</param>
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

        if (reducedHours.HasValue && reducedHours.Value.closing <= reducedHours.Value.opening)
            return null;

        if (reducedHours.HasValue)
            return reducedHours.Value;

        var schedule = GetApplicableSchedule(schedules, date);
        if (schedule == null)
            return null;

        return (schedule.OpeningTime, schedule.ClosingTime);
    }

    /// <summary>
    /// Generates all bookable time slots for a date, respecting closures and effective hours.
    /// Optionally filters by a specific court's closure status.
    /// </summary>
    /// <param name="schedules">The site's configured schedules.</param>
    /// <param name="closures">The site's closures.</param>
    /// <param name="date">The date to generate slots for.</param>
    /// <param name="courtId">Optional court ID to check for court-specific closures.</param>
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
        var slotDuration = TimeSpan.FromMinutes(SiteSchedule.SlotDurationMinutes);
        if (slotDuration <= TimeSpan.Zero)
            yield break;

        var step = TimeSpan.FromMinutes(SiteSchedule.SlotDurationMinutes + SiteSchedule.BreakDurationMinutes);
        if (step <= TimeSpan.Zero)
            yield break;

        var start = date.ToDateTime(openingTime);
        var closing = date.ToDateTime(closingTime);

        if (closing <= start)
            closing = closing.AddDays(1);

        while (true)
        {
            var end = start.Add(slotDuration);
            if (end > closing)
                yield break;

            yield return TimeSlot.FromDateTimes(start, end);

            start = start.Add(step);
            if (start >= closing)
                yield break;
        }
    }

    /// <summary>
    /// Validates that a new or updated schedule does not conflict with existing schedules.
    /// Schedules conflict when they overlap in date range, share the same priority, and have overlapping applicable days.
    /// </summary>
    /// <param name="newSchedule">The schedule to validate.</param>
    /// <param name="existingSchedules">Existing schedules to check against.</param>
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