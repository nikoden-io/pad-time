using PadTime.Domain.Common;

namespace PadTime.Domain.Booking;

/// <summary>
/// Represents a padel facility with one or more courts.
/// </summary>
public sealed class Site : AggregateRoot<Guid>
{
    private readonly List<Court> _courts = [];
    private readonly List<SiteYearSchedule> _schedules = [];
    private readonly List<Closure> _closures = [];

    public string Name { get; private set; } = null!;

    /// <summary>
    /// IANA timezone identifier (e.g., "Europe/Brussels").
    /// Used for displaying times to users.
    /// </summary>
    public string Timezone { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyList<Court> Courts => _courts.AsReadOnly();
    public IReadOnlyList<SiteYearSchedule> Schedules => _schedules.AsReadOnly();
    public IReadOnlyList<Closure> Closures => _closures.AsReadOnly();

    private Site() { } // EF Core

    public static Site Create(string name, string timezone, DateTime utcNow)
    {
        return new Site
        {
            Id = Guid.NewGuid(),
            Name = name,
            Timezone = timezone,
            IsActive = true,
            CreatedAtUtc = utcNow
        };
    }

    public Court AddCourt(string label, DateTime utcNow)
    {
        var court = Court.Create(Id, label, utcNow);
        _courts.Add(court);
        return court;
    }

    public void AddSchedule(SiteYearSchedule schedule)
    {
        // Ensure only one schedule per year
        if (_schedules.Any(s => s.Year == schedule.Year))
            throw new InvalidOperationException($"Schedule for year {schedule.Year} already exists.");

        _schedules.Add(schedule);
    }

    public void AddClosure(Closure closure)
    {
        _closures.Add(closure);
    }

    public SiteYearSchedule? GetScheduleForYear(int year)
    {
        return _schedules.FirstOrDefault(s => s.Year == year);
    }

    public bool IsClosedOn(DateOnly date)
    {
        return _closures.Any(c => c.Date == date);
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
