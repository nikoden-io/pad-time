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

        // ── Admin ────────────────────────────────────────────────────────
        EnsureUser(userManager, new ApplicationUser
        {
            UserName = "admin@test.be",
            Email = "admin@test.be",
            EmailConfirmed = true,
            Matricule = "G0001",
            MemberCategory = "global",
            Role = "admin_global",
            FirstName = "Admin",
            LastName = "System"
        });

        // ── Global members (G + 4 digits) ────────────────────────────────
        EnsureUser(userManager, new ApplicationUser
        {
            UserName = "alice@test.be",
            Email = "alice@test.be",
            EmailConfirmed = true,
            Matricule = "G1001",
            MemberCategory = "global",
            Role = "user",
            FirstName = "Alice",
            LastName = "Dupont"
        });

        EnsureUser(userManager, new ApplicationUser
        {
            UserName = "bob@test.be",
            Email = "bob@test.be",
            EmailConfirmed = true,
            Matricule = "G1002",
            MemberCategory = "global",
            Role = "user",
            FirstName = "Bob",
            LastName = "Martin"
        });

        EnsureUser(userManager, new ApplicationUser
        {
            UserName = "claire@test.be",
            Email = "claire@test.be",
            EmailConfirmed = true,
            Matricule = "G1003",
            MemberCategory = "global",
            Role = "user",
            FirstName = "Claire",
            LastName = "Leroy"
        });

        EnsureUser(userManager, new ApplicationUser
        {
            UserName = "david@test.be",
            Email = "david@test.be",
            EmailConfirmed = true,
            Matricule = "G1004",
            MemberCategory = "global",
            Role = "user",
            FirstName = "David",
            LastName = "Janssen"
        });

        EnsureUser(userManager, new ApplicationUser
        {
            UserName = "helene@test.be",
            Email = "helene@test.be",
            EmailConfirmed = true,
            Matricule = "G1005",
            MemberCategory = "global",
            Role = "user",
            FirstName = "Hélène",
            LastName = "Maes"
        });

        EnsureUser(userManager, new ApplicationUser
        {
            UserName = "kevin@test.be",
            Email = "kevin@test.be",
            EmailConfirmed = true,
            Matricule = "G1006",
            MemberCategory = "global",
            Role = "user",
            FirstName = "Kevin",
            LastName = "Wouters"
        });

        EnsureUser(userManager, new ApplicationUser
        {
            UserName = "nathalie@test.be",
            Email = "nathalie@test.be",
            EmailConfirmed = true,
            Matricule = "G1007",
            MemberCategory = "global",
            Role = "user",
            FirstName = "Nathalie",
            LastName = "Petit"
        });

        // ── Site members (S + 5 digits) ──────────────────────────────────
        EnsureUser(userManager, new ApplicationUser
        {
            UserName = "emma@test.be",
            Email = "emma@test.be",
            EmailConfirmed = true,
            Matricule = "S10001",
            MemberCategory = "site",
            Role = "user",
            FirstName = "Emma",
            LastName = "Dubois"
        });

        EnsureUser(userManager, new ApplicationUser
        {
            UserName = "francois@test.be",
            Email = "francois@test.be",
            EmailConfirmed = true,
            Matricule = "S10002",
            MemberCategory = "site",
            Role = "user",
            FirstName = "François",
            LastName = "Lambert"
        });

        EnsureUser(userManager, new ApplicationUser
        {
            UserName = "ibrahim@test.be",
            Email = "ibrahim@test.be",
            EmailConfirmed = true,
            Matricule = "S10003",
            MemberCategory = "site",
            Role = "user",
            FirstName = "Ibrahim",
            LastName = "Yilmaz"
        });

        EnsureUser(userManager, new ApplicationUser
        {
            UserName = "lea@test.be",
            Email = "lea@test.be",
            EmailConfirmed = true,
            Matricule = "S10004",
            MemberCategory = "site",
            Role = "user",
            FirstName = "Léa",
            LastName = "Renard"
        });

        // ── Free members (L + 5 digits) ──────────────────────────────────
        // Georges = the debtor (45 € debt, blocked from creating matches)
        EnsureUser(userManager, new ApplicationUser
        {
            UserName = "georges@test.be",
            Email = "georges@test.be",
            EmailConfirmed = true,
            Matricule = "L10001",
            MemberCategory = "free",
            Role = "user",
            FirstName = "Georges",
            LastName = "Peeters"
        });

        EnsureUser(userManager, new ApplicationUser
        {
            UserName = "julie@test.be",
            Email = "julie@test.be",
            EmailConfirmed = true,
            Matricule = "L10002",
            MemberCategory = "free",
            Role = "user",
            FirstName = "Julie",
            LastName = "Claes"
        });

        EnsureUser(userManager, new ApplicationUser
        {
            UserName = "marc@test.be",
            Email = "marc@test.be",
            EmailConfirmed = true,
            Matricule = "L10003",
            MemberCategory = "free",
            Role = "user",
            FirstName = "Marc",
            LastName = "Hendrickx"
        });
    }

    private static void EnsureUser(UserManager<ApplicationUser> userManager, ApplicationUser user)
    {
        var existing = userManager.FindByEmailAsync(user.Email!).Result;
        if (existing != null)
            return;

        var result = userManager.CreateAsync(user, "Passw0rd!").Result;
        if (!result.Succeeded)
        {
            throw new Exception(
                $"Failed to create {user.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
}
