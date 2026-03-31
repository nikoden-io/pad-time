// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using System.Security.Claims;
using PadTime.Application.Common.Interfaces;
using PadTime.Domain.Members;

namespace PadTime.API.Services;

/// <summary>
/// Extracts the current user's identity and authorization claims from the HTTP context.
/// Implements <see cref="ICurrentUser"/> by reading JWT claims such as subject, matricule,
/// member category, site assignment, and role.
/// </summary>
public sealed class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentUserService"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">Accessor for the current HTTP context.</param>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    /// <inheritdoc />
    public string Subject => User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User?.FindFirstValue("sub")
        ?? string.Empty;

    /// <inheritdoc />
    public string Matricule => User?.FindFirstValue("matricule") ?? string.Empty;

    /// <inheritdoc />
    public MemberCategory Category
    {
        get
        {
            var category = User?.FindFirstValue("member_category");
            return category?.ToLowerInvariant() switch
            {
                "global" => MemberCategory.Global,
                "site" => MemberCategory.Site,
                "free" => MemberCategory.Free,
                _ => MemberCategory.Free
            };
        }
    }

    /// <inheritdoc />
    public Guid? SiteId
    {
        get
        {
            var siteId = User?.FindFirstValue("site_id");
            return Guid.TryParse(siteId, out var id) ? id : null;
        }
    }

    /// <inheritdoc />
    public string Role => User?.FindFirstValue(ClaimTypes.Role)
        ?? User?.FindFirstValue("role")
        ?? Domain.Members.Role.User;

    /// <inheritdoc />
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    /// <inheritdoc />
    public bool IsGlobalAdmin => Role == Domain.Members.Role.AdminGlobal;

    /// <inheritdoc />
    public bool IsSiteAdmin => Role == Domain.Members.Role.AdminSite;

    /// <inheritdoc />
    public bool IsAdmin => IsGlobalAdmin || IsSiteAdmin;
}