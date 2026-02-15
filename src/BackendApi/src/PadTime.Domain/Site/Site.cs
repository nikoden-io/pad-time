using PadTime.Domain.Booking;
using PadTime.Domain.Common;

namespace PadTime.Domain.Site;

/// <summary>
/// Represents a padel site with courts, schedules, and closures.
/// </summary>
public sealed class Site : AggregateRoot<Guid>
{
    private readonly List<Court> _courts = [];
    private readonly List<SiteSchedule> _schedules = [];
    private readonly List<SiteClosure> _closures = [];

    public string Name { get; private set; } = null!;
    public string StreetNumber { get; private set; } = null!;
    public string Street { get; private set; } = null!;
    public string Postcode { get; private set; } = null!;
    public string City { get; private set; } = null!;
    public string Country { get; private set; } = null!;

    /// <summary>
    /// IANA timezone identifier (e.g., "Europe/Brussels").
    /// Used to display times to users.
    /// </summary>
    public string Timezone { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public IReadOnlyList<Court> Courts => _courts.AsReadOnly();
    public IReadOnlyList<SiteSchedule> Schedules => _schedules.AsReadOnly();
    public IReadOnlyList<SiteClosure> Closures => _closures.AsReadOnly();

    private Site() { } // EF Core

    public static Site Create(
        string name,
        string streetNumber,
        string street,
        string postcode,
        string city,
        string country,
        string timezone,
        DateTime utcNow)
    {
        return new Site
        {
            Id = Guid.NewGuid(),
            Name = name,
            StreetNumber = streetNumber,
            Street = street,
            Postcode = postcode,
            City = city,
            Country = country,
            Timezone = timezone,
            IsActive = true,
            CreatedAtUtc = utcNow,
            Version = 1,
        };
    }

    /// <summary>
    /// Adds a court to the site.
    /// </summary>
    public Court AddCourt(string label, DateTime utcNow)
    {
        var court = Court.Create(Id, label, utcNow);
        _courts.Add(court);
        UpdatedAtUtc = utcNow;
        return court;
    }

    /// <summary>
    /// Updates a court's information.
    /// </summary>
    public Result UpdateCourt(Guid courtId, string label, DateTime utcNow)
    {
        var court = _courts.FirstOrDefault(c => c.Id == courtId);
        if (court == null)
            return DomainErrors.Court.NotFound;

        court.UpdateLabel(label);
        UpdatedAtUtc = utcNow;
        return Result.Success();
    }

    /// <summary>
    /// Removes a court from the site.
    /// Note: This should only be called after verifying no active bookings exist.
    /// </summary>
    public Result RemoveCourt(Guid courtId, DateTime utcNow)
    {
        var court = _courts.FirstOrDefault(c => c.Id == courtId);
        if (court == null)
            return DomainErrors.Court.NotFound;

        _courts.Remove(court);
        UpdatedAtUtc = utcNow;
        return Result.Success();
    }

    /// <summary>
    /// Adds a schedule to the site.
    /// Validates that there are no conflicts with existing schedules.
    /// </summary>
    public Result<SiteSchedule> AddSchedule(
        string name,
        DateOnly validFrom,
        DateOnly? validUntil,
        TimeOnly openingTime,
        TimeOnly closingTime,
        DayOfWeek[]? applicableDays,
        int priority,
        DateTime utcNow)
    {
        var scheduleResult = SiteSchedule.Create(
            Id, name, validFrom, validUntil,
            openingTime, closingTime, applicableDays, priority, utcNow);

        if (scheduleResult.IsFailure)
            return scheduleResult;

        var schedule = scheduleResult.Value;

        // Validate overlaps
        var validationResult = SiteAvailabilityService.ValidateScheduleOverlap(
            schedule, _schedules);

        if (validationResult.IsFailure)
            return validationResult.PadTimeError;

        _schedules.Add(schedule);
        UpdatedAtUtc = utcNow;

        return schedule;
    }

    /// <summary>
    /// Updates an existing schedule.
    /// Validates that there are no conflicts with other schedules.
    /// </summary>
    public Result<SiteSchedule> UpdateSchedule(
        Guid scheduleId,
        string name,
        DateOnly validFrom,
        DateOnly? validUntil,
        TimeOnly openingTime,
        TimeOnly closingTime,
        DayOfWeek[]? applicableDays,
        int priority,
        DateTime utcNow)
    {
        var schedule = _schedules.FirstOrDefault(s => s.Id == scheduleId);
        if (schedule == null)
            return DomainErrors.Site.ScheduleNotFound;

        // Update the schedule
        var updateResult = schedule.Update(
            name, validFrom, validUntil,
            openingTime, closingTime, applicableDays, priority, utcNow);

        if (updateResult.IsFailure)
            return updateResult.PadTimeError;

        // Validate overlaps with OTHER schedules (exclude the current one)
        var otherSchedules = _schedules.Where(s => s.Id != scheduleId).ToList();
        var validationResult = SiteAvailabilityService.ValidateScheduleOverlap(
            schedule, otherSchedules);

        if (validationResult.IsFailure)
            return validationResult.PadTimeError;

        UpdatedAtUtc = utcNow;

        return schedule;
    }

    /// <summary>
    /// Removes a schedule from the site.
    /// </summary>
    public Result RemoveSchedule(Guid scheduleId, DateTime utcNow)
    {
        var schedule = _schedules.FirstOrDefault(s => s.Id == scheduleId);
        if (schedule == null)
            return DomainErrors.Site.ScheduleNotFound;

        _schedules.Remove(schedule);
        UpdatedAtUtc = utcNow;

        return Result.Success();
    }

    /// <summary>
    /// Adds a full day closure.
    /// </summary>
    public Result<SiteClosure> AddFullDayClosure(
        DateOnly date,
        ClosureReason reason,
        string? description,
        Guid[]? affectedCourtIds,
        DateTime utcNow)
    {
        var closureResult = SiteClosure.CreateFullDayClosure(
            Id, date, reason, description, affectedCourtIds, utcNow);

        if (closureResult.IsFailure)
            return closureResult;

        _closures.Add(closureResult.Value);
        UpdatedAtUtc = utcNow;

        return closureResult;
    }

    /// <summary>
    /// Adds a period closure (vacation, maintenance, etc.).
    /// </summary>
    public Result<SiteClosure> AddPeriodClosure(
        DateOnly startDate,
        DateOnly endDate,
        ClosureReason reason,
        string? description,
        Guid[]? affectedCourtIds,
        DateTime utcNow)
    {
        var closureResult = SiteClosure.CreatePeriodClosure(
            Id, startDate, endDate, reason, description, affectedCourtIds, utcNow);

        if (closureResult.IsFailure)
            return closureResult;

        _closures.Add(closureResult.Value);
        UpdatedAtUtc = utcNow;

        return closureResult;
    }

    /// <summary>
    /// Adds reduced hours for a specific day.
    /// </summary>
    public Result<SiteClosure> AddReducedHoursClosure(
        DateOnly date,
        TimeOnly modifiedOpeningTime,
        TimeOnly modifiedClosingTime,
        ClosureReason reason,
        string? description,
        Guid[]? affectedCourtIds,
        DateTime utcNow)
    {
        var closureResult = SiteClosure.CreateReducedHours(
            Id, date, modifiedOpeningTime, modifiedClosingTime,
            reason, description, affectedCourtIds, utcNow);

        if (closureResult.IsFailure)
            return closureResult;

        _closures.Add(closureResult.Value);
        UpdatedAtUtc = utcNow;

        return closureResult;
    }

    /// <summary>
    /// Removes a closure.
    /// </summary>
    public Result RemoveClosure(Guid closureId, DateTime utcNow)
    {
        var closure = _closures.FirstOrDefault(c => c.Id == closureId);
        if (closure == null)
            return DomainErrors.Site.ClosureNotFound;

        _closures.Remove(closure);
        UpdatedAtUtc = utcNow;

        return Result.Success();
    }

    /// <summary>
    /// Checks if the site is closed on a given date.
    /// </summary>
    public bool IsClosedOn(DateOnly date)
    {
        return SiteAvailabilityService.IsSiteClosed(_closures, date);
    }

    /// <summary>
    /// Checks if a specific court is closed on a given date.
    /// </summary>
    public bool IsCourtClosedOn(Guid courtId, DateOnly date)
    {
        return SiteAvailabilityService.IsCourtClosed(_closures, date, courtId);
    }

    /// <summary>
    /// Gets the applicable schedule for a given date.
    /// </summary>
    public SiteSchedule? GetScheduleForDate(DateOnly date)
    {
        return SiteAvailabilityService.GetApplicableSchedule(_schedules, date);
    }

    /// <summary>
    /// Gets the effective operating hours (with reduced hours if applicable).
    /// </summary>
    public (TimeOnly opening, TimeOnly closing)? GetEffectiveHoursForDate(DateOnly date)
    {
        return SiteAvailabilityService.GetEffectiveHours(_schedules, _closures, date);
    }

    /// <summary>
    /// Generates all available time slots for a given date.
    /// </summary>
    public IEnumerable<TimeSlot> GetAvailableSlots(DateOnly date, Guid? courtId = null)
    {
        return SiteAvailabilityService.GetAvailableSlots(_schedules, _closures, date, courtId);
    }

    /// <summary>
    /// Gets the site's timezone.
    /// </summary>
    public TimeZoneInfo GetTimeZone()
    {
        return TimeZoneInfo.FindSystemTimeZoneById(Timezone);
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

    /// <summary>
    /// Updates the site information.
    /// </summary>
    public void UpdateInformation(
        string name,
        string streetNumber,
        string street,
        string postcode,
        string city,
        string country,
        string timezone,
        DateTime utcNow)
    {
        Name = name;
        StreetNumber = streetNumber;
        Street = street;
        Postcode = postcode;
        City = city;
        Country = country;
        Timezone = timezone;
        UpdatedAtUtc = utcNow;
    }
}
