// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using Microsoft.AspNetCore.Authorization;

namespace PadTime.API.Authorization.Requirements;

/// <summary>
/// Authorization requirement for site management operations.
/// Users must be either a global admin or a site admin with management permissions.
/// </summary>
public class SiteManagementRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SiteManagementRequirement"/> class.
    /// </summary>
    /// <param name="siteIdParameterName">
    /// The name of the route or query parameter containing the site identifier. Defaults to "siteId".
    /// </param>
    public SiteManagementRequirement(string siteIdParameterName = "siteId")
    {
        SiteIdParameterName = siteIdParameterName;
    }

    /// <summary>
    /// Gets the name of the request parameter used to extract the site identifier.
    /// </summary>
    public string SiteIdParameterName { get; }
}