using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityServer.Services;

public class CustomProfileService : IProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CustomProfileService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var user = await _userManager.GetUserAsync(context.Subject);
        if (user != null)
        {
            var claims = new List<Claim>
            {
                new Claim("matricule", user.Matricule),
                new Claim("member_category", user.MemberCategory),
                new Claim("role", user.Role),
                new Claim("given_name", user.FirstName),
                new Claim("family_name", user.LastName)
            };

            if (!string.IsNullOrEmpty(user.SiteId))
            {
                claims.Add(new Claim("site_id", user.SiteId));
            }

            context.IssuedClaims.AddRange(claims);
        }
    }

    public Task IsActiveAsync(IsActiveContext context)
    {
        context.IsActive = true;
        return Task.CompletedTask;
    }
}