using Microsoft.EntityFrameworkCore;
using PadTime.Domain.Billing;
using PadTime.Domain.Booking;
using PadTime.Domain.Members;

namespace PadTime.Infrastructure.Persistence;

/// <summary>
/// Seeds realistic demo data at startup (development only).
/// Re-runs on every startup: cleans old demo data, then recreates with dates
/// relative to today so dashboards always show fresh content.
///
/// Produces:
///   • 7 demo members (Global / Site / Free categories)
///   • ~30 completed matches spread over the last 6 months → revenue in analytics
///   • 2 completed today → KPI "revenue today"
///   • 1 incomplete match with organizer debt
///   • 1 cancelled match
///   • Upcoming matches triggering overview alerts (J-1, unpaid, full)
/// </summary>
public static class DemoSeeder
{
    private const string DemoSubjectPrefix = "demo|";

    public static void SeedDemoData(PadTimeDbContext context)
    {
        CleanDemoData(context);
        CreateDemoData(context);
    }

    private static void CleanDemoData(PadTimeDbContext context)
    {
        var demoMemberIds = context.Members
            .Where(m => m.Subject.StartsWith(DemoSubjectPrefix))
            .Select(m => m.Id)
            .ToList();

        if (demoMemberIds.Count == 0)
            return;

        // Delete in FK order: payments → debts → matches (cascade deletes participants) → members
        var demoMatchIds = context.Matches
            .Where(m => demoMemberIds.Contains(m.OrganizerId))
            .Select(m => m.Id)
            .ToList();

        if (demoMatchIds.Count > 0)
        {
            context.Payments
                .Where(p => demoMatchIds.Contains(p.MatchId))
                .ExecuteDelete();
        }

        context.OrganizerDebts
            .Where(d => demoMemberIds.Contains(d.MemberId))
            .ExecuteDelete();

        if (demoMatchIds.Count > 0)
        {
            // Participants cascade-delete with matches
            context.Matches
                .Where(m => demoMatchIds.Contains(m.Id))
                .ExecuteDelete();
        }

        context.Members
            .Where(m => m.Subject.StartsWith(DemoSubjectPrefix))
            .ExecuteDelete();
    }

    private static void CreateDemoData(PadTimeDbContext context)
    {
        var now = DateTime.UtcNow;

        // ── Sites & courts (already seeded by DbSeeder) ───────────────────
        var brussels = context.Sites.Include(s => s.Courts)
                          .First(s => s.Name == "Brussels Padel Center");
        var liege = context.Sites.Include(s => s.Courts)
                       .First(s => s.Name == "Liège Sport Complex");

        var bxl = brussels.Courts.OrderBy(c => c.Label).ToList();
        var lge = liege.Courts.OrderBy(c => c.Label).ToList();

        // ── Members ───────────────────────────────────────────────────────
        var alice    = AddMember(context, "demo|alice",    "G1001", null,        now);
        var bob      = AddMember(context, "demo|bob",      "G1002", null,        now);
        var claire   = AddMember(context, "demo|claire",   "G1003", null,        now);
        var david    = AddMember(context, "demo|david",    "G1004", null,        now);
        var emma     = AddMember(context, "demo|emma",     "S10001", brussels.Id, now);
        var francois = AddMember(context, "demo|francois", "S10002", liege.Id,    now);
        var georges  = AddMember(context, "demo|georges",  "L10001", null,        now);

        context.SaveChanges();

        var allPlayers = new[] { alice, bob, claire, david, emma, francois, georges };

        // ── Helper ───────────────────────────────────────────────────────
        DateTime MatchDay(int offsetDays, int hourUtc = 7) =>
            now.Date.AddDays(offsetDays).AddHours(hourUtc);

        // ================================================================
        // PAST MATCHES — last 6 months of revenue for analytics
        // ================================================================
        // Generate ~4 matches per month across both sites for a realistic curve.
        // Alternates courts and organizers for variety.

        var courtRotation = new[]
        {
            (brussels.Id, bxl[0].Id),
            (brussels.Id, bxl[1].Id),
            (liege.Id,    lge[0].Id),
            (brussels.Id, bxl[2].Id),
            (liege.Id,    lge[1].Id),
        };

        var organizerRotation = new[] { alice, bob, claire, david, emma, francois, georges };

        // Spread matches: roughly every 5 days over last 180 days
        var pastDays = new List<int>();
        for (var d = -180; d <= -2; d += 5)
            pastDays.Add(d);

        // Add some clustering in recent weeks (more activity = more realistic)
        for (var d = -28; d <= -2; d += 3)
        {
            if (!pastDays.Contains(d))
                pastDays.Add(d);
        }

        pastDays.Sort();

        for (var i = 0; i < pastDays.Count; i++)
        {
            var day = pastDays[i];
            var (siteId, courtId) = courtRotation[i % courtRotation.Length];
            var organizer = organizerRotation[i % organizerRotation.Length];

            // Pick 3 other players (skip organizer)
            var others = allPlayers
                .Where(p => p.Id != organizer.Id)
                .OrderBy(_ => (i + day).GetHashCode()) // deterministic shuffle
                .Take(3)
                .Select(p => p.Id)
                .ToArray();

            SeedFullMatch(context, siteId, courtId,
                organizer.Id, others, MatchDay(day));
        }

        // ── TODAY's matches — for KPI "revenue today" / "matches today" ──

        // [BXL C1] Today 09:00 — completed this morning
        SeedFullMatch(context, brussels.Id, bxl[0].Id,
            alice.Id, [bob.Id, claire.Id, david.Id], MatchDay(0, 7));

        // [LGE C1] Today 11:00 — completed at noon
        SeedFullMatch(context, liege.Id, lge[0].Id,
            francois.Id, [alice.Id, georges.Id, bob.Id], MatchDay(0, 9));

        // ── Incomplete match — organizer debt ────────────────────────────
        // [BXL C3] -8 d — Bob org, David excluded → Bob owes 15 €
        SeedIncompleteMatch(context, brussels.Id, bxl[2].Id,
            organizerId:  bob.Id,
            paidPlayers:  [alice.Id, claire.Id],
            unpaidPlayer: david.Id,
            startAt: MatchDay(-8));

        context.OrganizerDebts.Add(
            OrganizerDebt.Create(bob.Id, 1500, MatchDay(-8)));

        // ── Cancelled match ──────────────────────────────────────────────
        // [BXL C2] -1 d — Georges org, cancelled
        {
            var start = MatchDay(-1);
            var m = Match.Create(brussels.Id, bxl[1].Id, georges.Id,
                start, start.AddMinutes(90), PadMatchType.Public,
                start.AddHours(-10)).Value;
            m.Cancel(start.AddHours(-6));
            context.Matches.Add(m);
        }

        // ================================================================
        // UPCOMING MATCHES — overview alerts
        // ================================================================

        // [BXL C2] +1 d — PRIVATE, David org + Alice · UNPAID → J-1 alert!
        {
            var start = MatchDay(1);
            var creation = now.AddHours(-3);
            var m = Match.Create(brussels.Id, bxl[1].Id, david.Id,
                start, start.AddMinutes(90), PadMatchType.Private, creation).Value;
            m.AddParticipant(alice.Id, creation);
            context.Matches.Add(m);
        }

        // [LGE C1] +1 d — PRIVATE, François org + Georges · UNPAID → J-1 alert (Liège)
        {
            var start = MatchDay(1, 9);
            var creation = now.AddHours(-5);
            var m = Match.Create(liege.Id, lge[0].Id, francois.Id,
                start, start.AddMinutes(90), PadMatchType.Private, creation).Value;
            m.AddParticipant(georges.Id, creation);
            context.Matches.Add(m);
        }

        // [BXL C1] +3 d — PUBLIC, Claire org + Georges joined (unpaid) → unpaid alert
        {
            var start = MatchDay(3);
            var creation = now.AddHours(-4);
            var m = Match.Create(brussels.Id, bxl[0].Id, claire.Id,
                start, start.AddMinutes(90), PadMatchType.Public, creation).Value;
            m.JoinPublic(georges.Id, creation);
            context.Matches.Add(m);
        }

        // [LGE C1] +5 d — PUBLIC, François org + 3 others · ALL PAID → Full
        {
            var start = MatchDay(5);
            var creation = now.AddHours(-8);
            var m = Match.Create(liege.Id, lge[0].Id, francois.Id,
                start, start.AddMinutes(90), PadMatchType.Public, creation).Value;
            var r2 = m.JoinPublic(alice.Id,  creation).Value;
            var r3 = m.JoinPublic(bob.Id,    creation).Value;
            var r4 = m.JoinPublic(claire.Id, creation).Value;

            var org = m.Participants.First(p => p.Role == ParticipantRole.Organizer);
            m.ConfirmPayment(org.Id, creation);
            m.ConfirmPayment(r2.Id,  creation);
            m.ConfirmPayment(r3.Id,  creation);
            m.ConfirmPayment(r4.Id,  creation);

            foreach (var p in m.Participants)
                AddPaidPayment(context, m.Id, p.MemberId, p.Id, creation);

            context.Matches.Add(m);
        }

        // [LGE C2] +6 d — PRIVATE, Alice org + Bob · UNPAID → unpaid alert (Liège)
        {
            var start = MatchDay(6);
            var creation = now.AddHours(-5);
            var m = Match.Create(liege.Id, lge[1].Id, alice.Id,
                start, start.AddMinutes(90), PadMatchType.Private, creation).Value;
            m.AddParticipant(bob.Id, creation);
            context.Matches.Add(m);
        }

        // [BXL C3] +14 d — PUBLIC, Georges org · open
        {
            var start = MatchDay(14);
            var creation = now.AddHours(-1);
            var m = Match.Create(brussels.Id, bxl[2].Id, georges.Id,
                start, start.AddMinutes(90), PadMatchType.Public, creation).Value;
            context.Matches.Add(m);
        }

        // [BXL C4] +21 d — PUBLIC, Emma org + David joined · 2/4 open
        {
            var start = MatchDay(21);
            var creation = now.AddMinutes(-45);
            var m = Match.Create(brussels.Id, bxl[3].Id, emma.Id,
                start, start.AddMinutes(90), PadMatchType.Public, creation).Value;
            m.JoinPublic(david.Id, creation);
            context.Matches.Add(m);
        }

        context.SaveChanges();
    }

    // ─────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────

    private static Member AddMember(
        PadTimeDbContext context,
        string subject, string matricule, Guid? siteId, DateTime now)
    {
        var m = Member.Create(subject, matricule, siteId, now).Value;
        context.Members.Add(m);
        return m;
    }

    private static void SeedFullMatch(
        PadTimeDbContext context,
        Guid siteId, Guid courtId,
        Guid organizerId, Guid[] players,
        DateTime startAt)
    {
        var creation = startAt.AddHours(-10);

        var m = Match.Create(siteId, courtId, organizerId,
            startAt, startAt.AddMinutes(90),
            PadMatchType.Public, creation).Value;

        foreach (var pid in players)
            m.JoinPublic(pid, creation);

        foreach (var part in m.Participants.ToList())
        {
            m.ConfirmPayment(part.Id, creation);
            AddPaidPayment(context, m.Id, part.MemberId, part.Id, creation);
        }

        m.Lock(startAt.AddMinutes(5));
        m.Complete(startAt.AddMinutes(95));

        context.Matches.Add(m);
    }

    private static void SeedIncompleteMatch(
        PadTimeDbContext context,
        Guid siteId, Guid courtId,
        Guid organizerId, Guid[] paidPlayers, Guid unpaidPlayer,
        DateTime startAt)
    {
        var creation = startAt.AddHours(-10);

        var m = Match.Create(siteId, courtId, organizerId,
            startAt, startAt.AddMinutes(90),
            PadMatchType.Private, creation).Value;

        foreach (var pid in paidPlayers)
            m.AddParticipant(pid, creation);
        m.AddParticipant(unpaidPlayer, creation);

        foreach (var part in m.Participants
                     .Where(p => p.MemberId != unpaidPlayer)
                     .ToList())
        {
            m.ConfirmPayment(part.Id, creation);
            AddPaidPayment(context, m.Id, part.MemberId, part.Id, creation);
        }

        m.ExcludeUnpaidParticipants(startAt.AddHours(-1));
        m.Lock(startAt.AddMinutes(5));
        m.Complete(startAt.AddMinutes(95));

        context.Matches.Add(m);
    }

    private static void AddPaidPayment(
        PadTimeDbContext context,
        Guid matchId, Guid memberId, Guid participantId,
        DateTime createdAt)
    {
        var p = Payment.Create(
            matchId, memberId, participantId,
            Match.PricePerParticipantCents,
            PaymentPurpose.MatchParticipation,
            Guid.NewGuid().ToString(),
            createdAt).Value;

        p.MarkAsPaid(createdAt.AddSeconds(30));
        context.Payments.Add(p);
    }
}
