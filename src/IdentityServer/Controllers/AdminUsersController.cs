using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Controllers;

[ApiController]
[Route("api/admin/users")]
public class AdminUsersController(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration) : ControllerBase
{
    private static readonly HashSet<string> ValidRoles = ["user", "admin_site", "admin_global"];

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!IsAuthorized())
            return Unauthorized();

        var users = await userManager.Users
            .Select(u => new AdminUserDto(
                u.Id,
                u.Email!,
                u.FirstName,
                u.LastName,
                u.Matricule,
                u.MemberCategory,
                u.Role,
                u.SiteId))
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        if (!IsAuthorized())
            return Unauthorized();

        var user = await userManager.FindByIdAsync(id);

        if (user is null)
            return NotFound();

        return Ok(new AdminUserDto(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.Matricule,
            user.MemberCategory,
            user.Role,
            user.SiteId));
    }

    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateRoleRequest request)
    {
        if (!IsAuthorized())
            return Unauthorized();

        if (!ValidRoles.Contains(request.Role))
            return BadRequest(new { message = $"Invalid role. Valid roles: {string.Join(", ", ValidRoles)}" });

        var user = await userManager.FindByIdAsync(id);

        if (user is null)
            return NotFound();

        user.Role = request.Role;

        if (request.Role == "admin_global")
            user.MemberCategory = "global";

        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return Ok(new AdminUserDto(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.Matricule,
            user.MemberCategory,
            user.Role,
            user.SiteId));
    }

    private bool IsAuthorized()
    {
        var key = Request.Headers["X-Admin-Key"].FirstOrDefault();
        var expectedKey = configuration["AdminApi:Key"] ?? "dev-admin-key";
        return key == expectedKey;
    }
}

public record AdminUserDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string Matricule,
    string MemberCategory,
    string Role,
    string? SiteId);

public record UpdateRoleRequest(string Role);
