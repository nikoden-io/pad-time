using Microsoft.AspNetCore.Authorization;

namespace PadTime.API.Authorization.Requirements;

/// <summary>
/// Authorization requirement for site management operations.
/// Users must be either a global admin or a site admin with management permissions.
/// </summary>
public class SiteManagementRequirement : IAuthorizationRequirement
{
    public SiteManagementRequirement(string siteIdParameterName = "siteId")
    {
        SiteIdParameterName = siteIdParameterName;
    }

    public string SiteIdParameterName { get; }
}