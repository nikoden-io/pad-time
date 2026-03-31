// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using Microsoft.AspNetCore.Authorization;

namespace PadTime.API.Authorization.Requirements;

/// <summary>
/// Authorization requirement for site-specific access.
/// Users must be either a global admin or a site admin for the specific site.
/// </summary>
public class SiteAccessRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SiteAccessRequirement"/> class.
    /// </summary>
    /// <param name="siteIdParameterName">
    /// The name of the route or query parameter containing the site identifier. Defaults to "siteId".
    /// </param>
    public SiteAccessRequirement(string siteIdParameterName = "siteId")
    {
        SiteIdParameterName = siteIdParameterName;
    }

    /// <summary>
    /// Gets the name of the request parameter used to extract the site identifier.
    /// </summary>
    public string SiteIdParameterName { get; }
}