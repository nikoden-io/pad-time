using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace IdentityServer.Services;

public class CustomClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        
        identity.AddClaim(new Claim("matricule", user.Matricule));
        identity.AddClaim(new Claim("member_category", user.MemberCategory));
        identity.AddClaim(new Claim("role", user.Role));
        identity.AddClaim(new Claim("given_name", user.FirstName));
        identity.AddClaim(new Claim("family_name", user.LastName));
        
        if (!string.IsNullOrEmpty(user.SiteId))
        {
            identity.AddClaim(new Claim("site_id", user.SiteId));
        }

        return identity;
    }
}