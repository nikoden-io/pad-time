// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
namespace PadTime.Domain.Members;

/// <summary>
/// Application roles for authorization.
/// </summary>
public static class Role
{
    /// <summary>
    /// Standard member role. Can book courts and participate in matches.
    /// </summary>
    public const string User = "user";

    /// <summary>
    /// Site administrator role. Can manage courts, schedules, and closures for their assigned site.
    /// </summary>
    public const string AdminSite = "admin_site";

    /// <summary>
    /// Global administrator role. Can manage all sites and perform platform-wide operations.
    /// </summary>
    public const string AdminGlobal = "admin_global";
}