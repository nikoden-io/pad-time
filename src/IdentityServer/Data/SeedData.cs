using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Data;

public class SeedData
{
    public static void EnsureSeedData(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        context.Database.Migrate();

        // Admin user
        var admin = userManager.FindByEmailAsync("admin@test.be").Result;
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = "admin@test.be",
                Email = "admin@test.be",
                EmailConfirmed = true,
                Matricule = "A0001",
                MemberCategory = "global",
                Role = "admin_global",
                FirstName = "Admin",
                LastName = "System"
            };
            var result = userManager.CreateAsync(admin, "Passw0rd!").Result;
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        // Player user
        var player = userManager.FindByEmailAsync("player@test.be").Result;
        if (player == null)
        {
            player = new ApplicationUser
            {
                UserName = "player@test.be",
                Email = "player@test.be",
                EmailConfirmed = true,
                Matricule = "L0001",
                MemberCategory = "free",
                Role = "user",
                FirstName = "Player",
                LastName = "Test"
            };
            var result = userManager.CreateAsync(player, "Passw0rd!").Result;
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create player: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}