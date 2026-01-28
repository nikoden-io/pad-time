using System.Security.Claims;
using PadTime.Application.Common.Interfaces;
using PadTime.Domain.Members;

namespace PadTime.API.Services;

public sealed class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string Subject => User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User?.FindFirstValue("sub")
        ?? string.Empty;

    public string Matricule => User?.FindFirstValue("matricule") ?? string.Empty;

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

    public Guid? SiteId
    {
        get
        {
            var siteId = User?.FindFirstValue("site_id");
            return Guid.TryParse(siteId, out var id) ? id : null;
        }
    }

    public string Role => User?.FindFirstValue(ClaimTypes.Role)
        ?? User?.FindFirstValue("role")
        ?? Domain.Members.Role.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsGlobalAdmin => Role == Domain.Members.Role.AdminGlobal;

    public bool IsSiteAdmin => Role == Domain.Members.Role.AdminSite;

    public bool IsAdmin => IsGlobalAdmin || IsSiteAdmin;
}
