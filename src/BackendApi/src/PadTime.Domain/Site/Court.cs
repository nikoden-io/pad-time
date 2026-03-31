// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Common;

namespace PadTime.Domain.Site;

/// <summary>
/// Represents a single padel court within a site.
/// </summary>
public sealed class Court : Entity<Guid>
{
    /// <summary>
    /// The site this court belongs to.
    /// </summary>
    public Guid SiteId { get; private set; }

    /// <summary>
    /// Display label (e.g., "Court 1", "Court A").
    /// </summary>
    public string Label { get; private set; } = null!;

    /// <summary>
    /// Whether this court is currently available for bookings.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// When the court was created (UTC).
    /// </summary>
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

    /// <summary>
    /// Marks the court as inactive, preventing new bookings.
    /// </summary>
    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Marks the court as active, allowing new bookings.
    /// </summary>
    public void Activate() => IsActive = true;

    /// <summary>
    /// Updates the court's label.
    /// </summary>
    public void UpdateLabel(string label)
    {
        Label = label;
    }
}