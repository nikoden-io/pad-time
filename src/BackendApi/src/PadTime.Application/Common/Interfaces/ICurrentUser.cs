using PadTime.Domain.Members;

namespace PadTime.Application.Common.Interfaces;

/// <summary>
/// Provides access to the current authenticated user's claims.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// OIDC subject identifier.
    /// </summary>
    string Subject { get; }

    /// <summary>
    /// Business identifier (matricule).
    /// </summary>
    string Matricule { get; }

    /// <summary>
    /// Member category (Global, Site, Free).
    /// </summary>
    MemberCategory Category { get; }

    /// <summary>
    /// Assigned site ID (for site members and site admins).
    /// </summary>
    Guid? SiteId { get; }

    /// <summary>
    /// User's role.
    /// </summary>
    string Role { get; }

    /// <summary>
    /// Whether the user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Whether the user is a global admin.
    /// </summary>
    bool IsGlobalAdmin { get; }

    /// <summary>
    /// Whether the user is a site admin.
    /// </summary>
    bool IsSiteAdmin { get; }

    /// <summary>
    /// Whether the user is any type of admin.
    /// </summary>
    bool IsAdmin { get; }
}
