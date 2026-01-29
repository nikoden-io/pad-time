using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Models;

public class ApplicationUser : IdentityUser
{
    public string Matricule { get; set; } = default!;
    public string MemberCategory { get; set; } = "free"; // free|site|global
    public string Role { get; set; } = "user"; // user|admin_site|admin_global
    public string? SiteId { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
}
