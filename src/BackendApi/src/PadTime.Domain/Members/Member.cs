using PadTime.Domain.Common;

namespace PadTime.Domain.Members;

/// <summary>
/// Represents a member who can book courts and participate in matches.
/// Created from OIDC claims on first API access.
/// </summary>
public sealed class Member : AggregateRoot<Guid>
{
    /// <summary>
    /// OIDC subject identifier (from identity provider).
    /// </summary>
    public string Subject { get; private set; } = null!;

    /// <summary>
    /// Business identifier (Gxxxx, Sxxxxx, or Lxxxxx).
    /// </summary>
    public Matricule Matricule { get; private set; } = null!;

    /// <summary>
    /// Member category (derived from matricule).
    /// </summary>
    public MemberCategory Category => Matricule.Category;

    /// <summary>
    /// Site ID for site-restricted members (null for Global/Free).
    /// </summary>
    public Guid? SiteId { get; private set; }

    /// <summary>
    /// Whether the member account is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// When the member was created (UTC).
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// When the member was last updated (UTC).
    /// </summary>
    public DateTime? UpdatedAtUtc { get; private set; }

    private Member() { } // EF Core

    private Member(
        Guid id,
        string subject,
        Matricule matricule,
        Guid? siteId,
        DateTime createdAtUtc)
    {
        Id = id;
        Subject = subject;
        Matricule = matricule;
        SiteId = siteId;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
    }

    public static Result<Member> Create(
        string subject,
        string matriculeValue,
        Guid? siteId,
        DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(subject))
            return DomainErrors.Member.InvalidMatricule;

        var matriculeResult = Matricule.Create(matriculeValue);
        if (matriculeResult.IsFailure)
            return matriculeResult.PadTimeError;

        var matricule = matriculeResult.Value;

        // Site members must have a site ID
        if (matricule.Category == MemberCategory.Site && siteId is null)
            return DomainErrors.Booking.SiteScopeViolation;

        // Global/Free members should not have a site restriction
        if (matricule.Category != MemberCategory.Site)
            siteId = null;

        return new Member(
            Guid.NewGuid(),
            subject,
            matricule,
            siteId,
            utcNow);
    }

    /// <summary>
    /// Returns the number of days in advance this member can book.
    /// </summary>
    public int GetBookingWindowDays()
    {
        return Category switch
        {
            MemberCategory.Global => 21,
            MemberCategory.Site => 14,
            MemberCategory.Free => 5,
            _ => throw new InvalidOperationException($"Unknown category: {Category}")
        };
    }

    /// <summary>
    /// Checks if the member can book at the specified site.
    /// </summary>
    public bool CanBookAtSite(Guid siteId)
    {
        // Site members can only book at their assigned site
        if (Category == MemberCategory.Site)
            return SiteId == siteId;

        // Global and Free members can book anywhere
        return true;
    }

    /// <summary>
    /// Checks if the member can book for the specified date.
    /// </summary>
    public bool CanBookForDate(DateOnly matchDate, DateOnly today)
    {
        var maxDate = today.AddDays(GetBookingWindowDays());
        return matchDate <= maxDate;
    }

    public void Deactivate(DateTime utcNow)
    {
        IsActive = false;
        UpdatedAtUtc = utcNow;
    }

    public void Reactivate(DateTime utcNow)
    {
        IsActive = true;
        UpdatedAtUtc = utcNow;
    }
}
