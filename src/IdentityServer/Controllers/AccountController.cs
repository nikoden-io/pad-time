using IdentityServer.Data;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Check if email already exists
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return Conflict(new { message = "Email already in use" });

        // Generate next matricule (L0001, L0002, etc.)
        var nextMatricule = await GenerateNextMatricule();

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = false, // TODO: Email confirmation flow
            FirstName = request.FirstName,
            LastName = request.LastName,
            Matricule = nextMatricule,
            MemberCategory = "free",
            Role = "user"
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return Ok(new { matricule = user.Matricule, email = user.Email });
    }

    private async Task<string> GenerateNextMatricule()
    {
        var lastMatricule = await context.Users
            .Where(u => u.Matricule.StartsWith("L"))
            .OrderByDescending(u => u.Matricule)
            .Select(u => u.Matricule)
            .FirstOrDefaultAsync();

        if (lastMatricule == null)
            return "L0001";

        var number = int.Parse(lastMatricule.Substring(1));
        return $"L{number + 1:D4}";
    }
}

public record RegisterRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName
);