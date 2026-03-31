// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Common;

namespace PadTime.Domain.Site;

/// <summary>
/// Closure type.
/// </summary>
public enum ClosureType
{
    /// <summary>
    /// Full day closure.
    /// </summary>
    FullDay = 1,

    /// <summary>
    /// Period closure (multiple consecutive days).
    /// </summary>
    Period = 2,

    /// <summary>
    /// Reduced hours for a specific day.
    /// </summary>
    ReducedHours = 3
}

/// <summary>
/// Closure reason (for categorization and reporting).
/// </summary>
public enum ClosureReason
{
    /// <summary>
    /// Public holiday.
    /// </summary>
    PublicHoliday = 1,

    /// <summary>
    /// Vacation (annual leave).
    /// </summary>
    Vacation = 2,

    /// <summary>
    /// Maintenance or repair work.
    /// </summary>
    Maintenance = 3,

    /// <summary>
    /// Private event.
    /// </summary>
    PrivateEvent = 4,

    /// <summary>
    /// Adverse weather conditions.
    /// </summary>
    Weather = 5,

    /// <summary>
    /// Other reason.
    /// </summary>
    Other = 99
}

/// <summary>
/// Represents a closure or schedule modification for a site.
/// Can be a full closure, vacation period, or reduced hours.
/// </summary>
public sealed class SiteClosure : Entity<Guid>
{
    /// <summary>
    /// The site this closure applies to.
    /// </summary>
    public Guid SiteId { get; private set; }

    /// <summary>
    /// Closure type.
    /// </summary>
    public ClosureType Type { get; private set; }

    /// <summary>
    /// Closure reason.
    /// </summary>
    public ClosureReason Reason { get; private set; }

    /// <summary>
    /// Detailed description (optional).
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Closure start date (inclusive).
    /// </summary>
    public DateOnly StartDate { get; private set; }

    /// <summary>
    /// Closure end date (inclusive).
    /// For FullDay, equals StartDate.
    /// </summary>
    public DateOnly EndDate { get; private set; }

    /// <summary>
    /// Modified opening time for ReducedHours.
    /// Null for other types.
    /// </summary>
    public TimeOnly? ModifiedOpeningTime { get; private set; }

    /// <summary>
    /// Modified closing time for ReducedHours.
    /// Null for other types.
    /// </summary>
    public TimeOnly? ModifiedClosingTime { get; private set; }

    /// <summary>
    /// Specific courts affected by the closure.
    /// Null indicates all courts at the site.
    /// </summary>
    public Guid[]? AffectedCourtIds { get; private set; }

    /// <summary>
    /// When the closure was created (UTC).
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// When the closure was last modified (UTC).
    /// </summary>
    public DateTime? UpdatedAtUtc { get; private set; }

    private SiteClosure() { } // EF Core

    /// <summary>
    /// Creates a full day closure.
    /// </summary>
    public static Result<SiteClosure> CreateFullDayClosure(
        Guid siteId,
        DateOnly date,
        ClosureReason reason,
        string? description,
        Guid[]? affectedCourtIds,
        DateTime utcNow)
    {
        var closure = new SiteClosure
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            Type = ClosureType.FullDay,
            Reason = reason,
            Description = description,
            StartDate = date,
            EndDate = date,
            AffectedCourtIds = affectedCourtIds,
            CreatedAtUtc = utcNow
        };

        return closure;
    }

    /// <summary>
    /// Creates a period closure (multiple days).
    /// </summary>
    public static Result<SiteClosure> CreatePeriodClosure(
        Guid siteId,
        DateOnly startDate,
        DateOnly endDate,
        ClosureReason reason,
        string? description,
        Guid[]? affectedCourtIds,
        DateTime utcNow)
    {
        if (endDate < startDate)
            return DomainErrors.Site.InvalidClosure;

        var closure = new SiteClosure
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            Type = ClosureType.Period,
            Reason = reason,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            AffectedCourtIds = affectedCourtIds,
            CreatedAtUtc = utcNow
        };

        return closure;
    }

    /// <summary>
    /// Creates reduced hours for a specific day.
    /// </summary>
    public static Result<SiteClosure> CreateReducedHours(
        Guid siteId,
        DateOnly date,
        TimeOnly modifiedOpeningTime,
        TimeOnly modifiedClosingTime,
        ClosureReason reason,
        string? description,
        Guid[]? affectedCourtIds,
        DateTime utcNow)
    {
        if (modifiedClosingTime <= modifiedOpeningTime)
            return DomainErrors.Site.InvalidClosure;

        var closure = new SiteClosure
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            Type = ClosureType.ReducedHours,
            Reason = reason,
            Description = description,
            StartDate = date,
            EndDate = date,
            ModifiedOpeningTime = modifiedOpeningTime,
            ModifiedClosingTime = modifiedClosingTime,
            AffectedCourtIds = affectedCourtIds,
            CreatedAtUtc = utcNow
        };

        return closure;
    }

    /// <summary>
    /// Checks if this closure affects a given date.
    /// </summary>
    public bool AffectsDate(DateOnly date)
    {
        return date >= StartDate && date <= EndDate;
    }

    /// <summary>
    /// Checks if this closure affects a specific court.
    /// </summary>
    public bool AffectsCourt(Guid courtId)
    {
        // If AffectedCourtIds is null, all courts are affected
        if (AffectedCourtIds == null || AffectedCourtIds.Length == 0)
            return true;

        return AffectedCourtIds.Contains(courtId);
    }

    /// <summary>
    /// Checks if the site (or court) is fully closed on this date.
    /// </summary>
    public bool IsFullyClosed(DateOnly date, Guid? courtId = null)
    {
        if (!AffectsDate(date))
            return false;

        if (courtId.HasValue && !AffectsCourt(courtId.Value))
            return false;

        return Type is ClosureType.FullDay or ClosureType.Period;
    }

    /// <summary>
    /// Gets modified hours for a given date (if applicable).
    /// Returns null if not reduced hours or date is not affected.
    /// </summary>
    public (TimeOnly opening, TimeOnly closing)? GetModifiedHours(DateOnly date)
    {
        if (Type != ClosureType.ReducedHours)
            return null;

        if (!AffectsDate(date))
            return null;

        if (!ModifiedOpeningTime.HasValue || !ModifiedClosingTime.HasValue)
            return null;

        return (ModifiedOpeningTime.Value, ModifiedClosingTime.Value);
    }

    /// <summary>
    /// Extends the closure period.
    /// </summary>
    public Result ExtendPeriod(DateOnly newEndDate, DateTime utcNow)
    {
        if (Type == ClosureType.FullDay)
            return DomainErrors.Site.InvalidClosure;

        if (newEndDate < StartDate)
            return DomainErrors.Site.InvalidClosure;

        EndDate = newEndDate;
        UpdatedAtUtc = utcNow;

        return Result.Success();
    }

    /// <summary>
    /// Updates the description.
    /// </summary>
    public void UpdateDescription(string? description, DateTime utcNow)
    {
        Description = description;
        UpdatedAtUtc = utcNow;
    }
}