// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using Microsoft.AspNetCore.Authorization;
using PadTime.API.Authorization.Requirements;
using PadTime.Application.Common.Interfaces;
using PadTime.Domain.Members;

namespace PadTime.API.Authorization.Handlers;

/// <summary>
/// Authorization handler for site-specific access.
/// Allows global admins to access any site, and site admins to access their assigned site.
/// </summary>
public class SiteAccessHandler : AuthorizationHandler<SiteAccessRequirement>
{
    private readonly ICurrentUser _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="SiteAccessHandler"/> class.
    /// </summary>
    /// <param name="currentUser">Service providing the current authenticated user's claims.</param>
    /// <param name="httpContextAccessor">Accessor for the current HTTP context to extract route/query parameters.</param>
    public SiteAccessHandler(ICurrentUser currentUser, IHttpContextAccessor httpContextAccessor)
    {
        _currentUser = currentUser;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Evaluates whether the current user meets the <see cref="SiteAccessRequirement"/>.
    /// Global admins pass unconditionally; site admins pass only for their assigned site.
    /// </summary>
    /// <param name="context">The authorization handler context.</param>
    /// <param name="requirement">The site access requirement being evaluated.</param>
    /// <returns>A completed task once the requirement is evaluated.</returns>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SiteAccessRequirement requirement)
    {
        // Must be authenticated
        if (!_currentUser.IsAuthenticated)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // Global admins have access to all sites
        if (_currentUser.IsGlobalAdmin)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Site admins can only access their assigned site
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