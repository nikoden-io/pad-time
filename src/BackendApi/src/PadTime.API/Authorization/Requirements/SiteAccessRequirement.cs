using Microsoft.AspNetCore.Authorization;

namespace PadTime.API.Authorization.Requirements;

/// <summary>
/// Authorization requirement for site-specific access.
/// Users must be either a global admin or a site admin for the specific site.
/// </summary>
public class SiteAccessRequirement : IAuthorizationRequirement
{
    public SiteAccessRequirement(string siteIdParameterName = "siteId")
    {
        SiteIdParameterName = siteIdParameterName;
    }

    public string SiteIdParameterName { get; }
}