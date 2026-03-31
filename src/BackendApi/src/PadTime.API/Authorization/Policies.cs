// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
namespace PadTime.API.Authorization;

/// <summary>
/// Defines authorization policy names used throughout the API.
/// These constants are referenced by <c>[Authorize]</c> attributes on controllers and actions.
/// </summary>
public static class Policies
{
    /// <summary>Policy requiring an authenticated user.</summary>
    public const string RequireUser = "RequireUser";

    /// <summary>Policy requiring a site or global admin role.</summary>
    public const string RequireAdmin = "RequireAdmin";

    /// <summary>Policy requiring the global admin role.</summary>
    public const string RequireGlobalAdmin = "RequireGlobalAdmin";

    /// <summary>Policy requiring a site admin (or global admin) role.</summary>
    public const string RequireSiteAdmin = "RequireSiteAdmin";

    /// <summary>Policy requiring the user to have access to the requested site.</summary>
    public const string RequireSiteAccess = "RequireSiteAccess";

    /// <summary>Policy requiring management-level permissions for the requested site.</summary>
    public const string RequireSiteManagement = "RequireSiteManagement";
}