using PadTime.Domain.Common;

namespace PadTime.Domain.Booking;

/// <summary>
/// Represents a closure date for a site or globally.
/// </summary>
public sealed class Closure : Entity<Guid>
{
    /// <summary>
    /// Site ID if site-specific, null if global closure.
    /// </summary>
    public Guid? SiteId { get; private set; }

    /// <summary>
    /// The date of closure.
    /// </summary>
    public DateOnly Date { get; private set; }

    /// <summary>
    /// Reason for closure (e.g., "Public Holiday", "Maintenance").
    /// </summary>
    public string Reason { get; private set; } = null!;

    /// <summary>
    /// Whether this is a global closure affecting all sites.
    /// </summary>
    public bool IsGlobal => SiteId is null;

    private Closure() { } // EF Core

    public static Closure CreateForSite(Guid siteId, DateOnly date, string reason)
    {
        return new Closure
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            Date = date,
            Reason = reason
        };
    }

    public static Closure CreateGlobal(DateOnly date, string reason)
    {
        return new Closure
        {
            Id = Guid.NewGuid(),
            SiteId = null,
            Date = date,
            Reason = reason
        };
    }
}
