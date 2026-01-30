using PadTime.Domain.Booking;
using PadTime.Domain.Members;

namespace PadTime.Infrastructure.Persistence;

/// <summary>
/// Seeds initial data for development and testing.
/// </summary>
public static class DbSeeder
{
    public static void SeedData(PadTimeDbContext context)
    {
        // Skip if already seeded
        if (context.Sites.Any())
        {
            return;
        }

        var utcNow = DateTime.UtcNow;

        // ========================================
        // 1. SITES
        // ========================================
        var brusselsPadel = Site.Create(
            "Brussels Padel Center",
            "Europe/Brussels",
            utcNow);

        var liegeSport = Site.Create(
            "Liège Sport Complex",
            "Europe/Brussels",
            utcNow);

        var namurPadel = Site.Create(
            "Namur Padel Club",
            "Europe/Brussels",
            utcNow);

        context.Sites.AddRange(brusselsPadel, liegeSport, namurPadel);

        // ========================================
        // 2. COURTS (via Site.AddCourt)
        // ========================================

        // Brussels - 4 courts
        brusselsPadel.AddCourt("Court 1", utcNow);
        brusselsPadel.AddCourt("Court 2", utcNow);
        brusselsPadel.AddCourt("Court 3", utcNow);
        brusselsPadel.AddCourt("Court 4", utcNow);

        // Liège - 3 courts
        liegeSport.AddCourt("Court 1", utcNow);
        liegeSport.AddCourt("Court 2", utcNow);
        liegeSport.AddCourt("Court 3", utcNow);

        // Namur - 2 courts
        namurPadel.AddCourt("Court 1", utcNow);
        namurPadel.AddCourt("Court 2", utcNow);

        // ========================================
        // 3. SCHEDULES (2025 & 2026)
        // ========================================

        var schedule2025Brussels = SiteYearSchedule.Create(
            brusselsPadel.Id,
            2025,
            new TimeOnly(8, 0),
            new TimeOnly(22, 0));

        var schedule2026Brussels = SiteYearSchedule.Create(
            brusselsPadel.Id,
            2026,
            new TimeOnly(8, 0),
            new TimeOnly(22, 0));

        var schedule2025Liege = SiteYearSchedule.Create(
            liegeSport.Id,
            2025,
            new TimeOnly(9, 0),
            new TimeOnly(21, 0));

        var schedule2026Liege = SiteYearSchedule.Create(
            liegeSport.Id,
            2026,
            new TimeOnly(9, 0),
            new TimeOnly(21, 0));

        var schedule2025Namur = SiteYearSchedule.Create(
            namurPadel.Id,
            2025,
            new TimeOnly(10, 0),
            new TimeOnly(20, 0));

        var schedule2026Namur = SiteYearSchedule.Create(
            namurPadel.Id,
            2026,
            new TimeOnly(10, 0),
            new TimeOnly(20, 0));

        brusselsPadel.AddSchedule(schedule2025Brussels);
        brusselsPadel.AddSchedule(schedule2026Brussels);
        liegeSport.AddSchedule(schedule2025Liege);
        liegeSport.AddSchedule(schedule2026Liege);
        namurPadel.AddSchedule(schedule2025Namur);
        namurPadel.AddSchedule(schedule2026Namur);

        // ========================================
        // 4. CLOSURES (jours fériés 2026)
        // ========================================

        var closure1 = Closure.CreateForSite(
            brusselsPadel.Id,
            new DateOnly(2026, 1, 1),
            "New Year");

        var closure2 = Closure.CreateForSite(
            brusselsPadel.Id,
            new DateOnly(2026, 4, 6),
            "Easter Monday");

        var closure3 = Closure.CreateForSite(
            brusselsPadel.Id,
            new DateOnly(2026, 5, 1),
            "Labour Day");

        var closure4 = Closure.CreateForSite(
            brusselsPadel.Id,
            new DateOnly(2026, 5, 14),
            "Ascension Day");

        var closure5 = Closure.CreateForSite(
            brusselsPadel.Id,
            new DateOnly(2026, 7, 21),
            "Belgian National Day");

        var closure6 = Closure.CreateForSite(
            brusselsPadel.Id,
            new DateOnly(2026, 8, 15),
            "Assumption");

        var closure7 = Closure.CreateForSite(
            brusselsPadel.Id,
            new DateOnly(2026, 12, 25),
            "Christmas");

        brusselsPadel.AddClosure(closure1);
        brusselsPadel.AddClosure(closure2);
        brusselsPadel.AddClosure(closure3);
        brusselsPadel.AddClosure(closure4);
        brusselsPadel.AddClosure(closure5);
        brusselsPadel.AddClosure(closure6);
        brusselsPadel.AddClosure(closure7);

        // ========================================
        // SAVE ALL
        // ========================================
        context.SaveChanges();
    }
}
