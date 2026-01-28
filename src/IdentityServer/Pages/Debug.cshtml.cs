using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Debug;

[Authorize]
public class DebugModel : PageModel
{
    public void OnGet()
    {
        ViewData["Claims"] = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
    }
}