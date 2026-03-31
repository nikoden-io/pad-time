// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using Microsoft.AspNetCore.Authorization;
using PadTime.API.Authorization.Requirements;
using PadTime.Application.Common.Interfaces;
using PadTime.Domain.Members;

namespace PadTime.API.Authorization.Handlers;

/// <summary>
/// Authorization handler for site management operations.
/// Allows global admins to manage any site, and site admins to manage their assigned site.
/// </summary>
public class SiteManagementHandler : AuthorizationHandler<SiteManagementRequirement>
{
    private readonly ICurrentUser _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="SiteManagementHandler"/> class.
    /// </summary>
    /// <param name="currentUser">Service providing the current authenticated user's claims.</param>
    /// <param name="httpContextAccessor">Accessor for the current HTTP context to extract route/query parameters.</param>
    public SiteManagementHandler(ICurrentUser currentUser, IHttpContextAccessor httpContextAccessor)
    {
        _currentUser = currentUser;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Evaluates whether the current user meets the <see cref="SiteManagementRequirement"/>.
    /// Requires admin role. Global admins pass unconditionally; site admins pass only for their assigned site.
    /// </summary>
    /// <param name="context">The authorization handler context.</param>
    /// <param name="requirement">The site management requirement being evaluated.</param>
    /// <returns>A completed task once the requirement is evaluated.</returns>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SiteManagementRequirement requirement)
    {
        // Must be authenticated and have admin role
        if (!_currentUser.IsAuthenticated || !_currentUser.IsAdmin)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // Global admins can manage all sites
        if (_currentUser.IsGlobalAdmin)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Site admins can only manage their assigned site
        if (_currentUser.IsSiteAdmin)
        {
            var requestedSiteId = GetSiteIdFromRequest(requirement.SiteIdParameterName);
            if (requestedSiteId.HasValue && requestedSiteId == _currentUser.SiteId)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        context.Fail();
        return Task.CompletedTask;
    }

    private Guid? GetSiteIdFromRequest(string parameterName)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return null;

        // Try to get from route values
        if (httpContext.Request.RouteValues.TryGetValue(parameterName, out var routeValue) &&
            Guid.TryParse(routeValue?.ToString(), out var siteId))
        {
            return siteId;
        }

        // Try to get from query parameters
        if (httpContext.Request.Query.TryGetValue(parameterName, out var queryValue) &&
            Guid.TryParse(queryValue.FirstOrDefault(), out siteId))
        {
            return siteId;
        }

        return null;
    }
}