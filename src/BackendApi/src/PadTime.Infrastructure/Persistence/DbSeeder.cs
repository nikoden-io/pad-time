using PadTime.Domain.Site;

namespace PadTime.Infrastructure.Persistence;

public static class DbSeeder
{
    public static void SeedData(PadTimeDbContext context)
    {
        if (context.Sites.Any())
            return;

        var utcNow = DateTime.UtcNow;

        // ========================================
        // 1. SITES
        // ========================================
        var brusselsPadel = Site.Create(
            "Brussels Padel Center",
            "108",
            "Avenue Louise",
            "1000",
            "Bruxelles",
            "Belgium",
            "Europe/Brussels",
            utcNow);

        var liegeSport = Site.Create(
            "Liège Sport Complex",
            "10",
            "Boulevard d'Avroy",
            "4000",
            "Liège",
            "Belgium",
            "Europe/Brussels",
            utcNow);

        context.Sites.AddRange(brusselsPadel, liegeSport);

        // ========================================
        // 2. COURTS
        // ========================================
        brusselsPadel.AddCourt("Court 1", utcNow);
        brusselsPadel.AddCourt("Court 2", utcNow);
        brusselsPadel.AddCourt("Court 3", utcNow);
        brusselsPadel.AddCourt("Court 4", utcNow);

        liegeSport.AddCourt("Court 1", utcNow);
        liegeSport.AddCourt("Court 2", utcNow);
        liegeSport.AddCourt("Court 3", utcNow);

        // ========================================
        // 3. SCHEDULES (2025 & 2026)
        // ========================================

        // Brussels - Standard schedule
        brusselsPadel.AddSchedule(
            name: "Standard Schedule 2025-2026",
            validFrom: new DateOnly(2025, 1, 1),
            validUntil: new DateOnly(2026, 12, 31),
            openingTime: new TimeOnly(8, 0),
            closingTime: new TimeOnly(22, 0),
            applicableDays: null, // All days
            priority: 0,
            utcNow: utcNow);

        // Liège - Standard schedule
        liegeSport.AddSchedule(
            name: "Standard Schedule 2025-2026",
            validFrom: new DateOnly(2025, 1, 1),
            validUntil: new DateOnly(2026, 12, 31),
            openingTime: new TimeOnly(9, 0),
            closingTime: new TimeOnly(21, 0),
            applicableDays: null,
            priority: 0,
            utcNow: utcNow);

        // ========================================
        // 4. CLOSURES (Belgian holidays 2025-2026)
        // ========================================

        // 2025
        brusselsPadel.AddFullDayClosure(
            new DateOnly(2025, 1, 1),
            ClosureReason.PublicHoliday,
            "New Year's Day",
            null,
            utcNow);

        brusselsPadel.AddFullDayClosure(
            new DateOnly(2025, 4, 21),
            ClosureReason.PublicHoliday,
            "Easter Monday",
            null,
            utcNow);

        brusselsPadel.AddFullDayClosure(
            new DateOnly(2025, 5, 1),
            ClosureReason.PublicHoliday,
            "Labour Day",
            null,
            utcNow);

        brusselsPadel.AddFullDayClosure(
            new DateOnly(2025, 5, 29),
            ClosureReason.PublicHoliday,
            "Ascension Day",
            null,
            utcNow);

        brusselsPadel.AddFullDayClosure(
            new DateOnly(2025, 7, 21),
            ClosureReason.PublicHoliday,
            "Belgian National Day",
            null,
            utcNow);

        brusselsPadel.AddFullDayClosure(
            new DateOnly(2025, 8, 15),
            ClosureReason.PublicHoliday,
            "Assumption",
            null,
            utcNow);

        brusselsPadel.AddFullDayClosure(
            new DateOnly(2025, 12, 25),
            ClosureReason.PublicHoliday,
            "Christmas",
            null,
            utcNow);

        // 2026
        brusselsPadel.AddFullDayClosure(
            new DateOnly(2026, 1, 1),
            ClosureReason.PublicHoliday,
            "New Year's Day",
            null,
            utcNow);

        brusselsPadel.AddFullDayClosure(
            new DateOnly(2026, 4, 6),
            ClosureReason.PublicHoliday,
            "Easter Monday",
            null,
            utcNow);

        brusselsPadel.AddFullDayClosure(
            new DateOnly(2026, 5, 1),
            ClosureReason.PublicHoliday,
            "Labour Day",
            null,
            utcNow);

        brusselsPadel.AddFullDayClosure(
            new DateOnly(2026, 5, 14),
            ClosureReason.PublicHoliday,
            "Ascension Day",
            null,
            utcNow);

        brusselsPadel.AddFullDayClosure(
            new DateOnly(2026, 7, 21),
            ClosureReason.PublicHoliday,
            "Belgian National Day",
            null,
            utcNow);

        brusselsPadel.AddFullDayClosure(
            new DateOnly(2026, 8, 15),
            ClosureReason.PublicHoliday,
            "Assumption",
            null,
            utcNow);

        brusselsPadel.AddFullDayClosure(
            new DateOnly(2026, 12, 25),
            ClosureReason.PublicHoliday,
            "Christmas",
            null,
            utcNow);

        context.SaveChanges();
    }
}
