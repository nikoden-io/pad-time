using PadTime.Domain.Common;

namespace PadTime.Domain.Booking;

/// <summary>
/// Represents a single padel court within a site.
/// </summary>
public sealed class Court : Entity<Guid>
{
    public Guid SiteId { get; private set; }

    /// <summary>
    /// Display label (e.g., "Court 1", "Court A").
    /// </summary>
    public string Label { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private Court() { } // EF Core

    internal static Court Create(Guid siteId, string label, DateTime utcNow)
    {
        return new Court
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            Label = label,
            IsActive = true,
            CreatedAtUtc = utcNow
        };
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
