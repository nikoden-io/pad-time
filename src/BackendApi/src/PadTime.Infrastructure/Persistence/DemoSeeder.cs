using Microsoft.EntityFrameworkCore;
using PadTime.Domain.Billing;
using PadTime.Domain.Booking;
using PadTime.Domain.Members;

namespace PadTime.Infrastructure.Persistence;

/// <summary>
/// Seeds rich, realistic demo data at startup (development only).
/// Re-runs on every startup: cleans old demo data, then recreates with dates
/// relative to today so dashboards always show fresh content.
///
/// Produces:
///   • 14 demo members across Global / Site / Free categories
///   • ~80 completed matches spread over the last 6 months → revenue in analytics
///   • 4 completed today → KPI "revenue today"
///   • 3 incomplete matches → organizer debts
///   • 3 cancelled matches
///   • Georges Peeters (L10001) = blocked debtor (45 € debt, cannot create matches)
///   • Upcoming matches: private, public, paid, unpaid, full, open, J-1 alerts
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

        // ── Members (14) ─────────────────────────────────────────────────
        // Global members (G + 4 digits)
        var alice    = AddMember(context, "demo|alice",    "G1001", null,        now);
        var bob      = AddMember(context, "demo|bob",      "G1002", null,        now);
        var claire   = AddMember(context, "demo|claire",   "G1003", null,        now);
        var david    = AddMember(context, "demo|david",    "G1004", null,        now);
        var helene   = AddMember(context, "demo|helene",   "G1005", null,        now);
        var kevin    = AddMember(context, "demo|kevin",    "G1006", null,        now);
        var nathalie = AddMember(context, "demo|nathalie", "G1007", null,        now);

        // Site members (S + 5 digits)
        var emma     = AddMember(context, "demo|emma",     "S10001", brussels.Id, now);
        var francois = AddMember(context, "demo|francois", "S10002", liege.Id,    now);
        var ibrahim  = AddMember(context, "demo|ibrahim",  "S10003", brussels.Id, now);
        var lea      = AddMember(context, "demo|lea",      "S10004", liege.Id,    now);

        // Free members (L + 5 digits) — Georges is the debtor
        var georges  = AddMember(context, "demo|georges",  "L10001", null,        now);
        var julie    = AddMember(context, "demo|julie",    "L10002", null,        now);
        var marc     = AddMember(context, "demo|marc",     "L10003", null,        now);

        context.SaveChanges();

        var allPlayers = new[] { alice, bob, claire, david, helene, kevin, nathalie,
                                 emma, francois, ibrahim, lea, georges, julie, marc };

        // ── Helpers ──────────────────────────────────────────────────────
        DateTime MatchDay(int offsetDays, int hourUtc = 7) =>
            now.Date.AddDays(offsetDays).AddHours(hourUtc);

        // ================================================================
        // PAST MATCHES — 6 months of revenue for rich analytics
        // ================================================================

        var courtRotation = new[]
        {
            (brussels.Id, bxl[0].Id), (brussels.Id, bxl[1].Id),
            (liege.Id,    lge[0].Id), (brussels.Id, bxl[2].Id),
            (liege.Id,    lge[1].Id), (brussels.Id, bxl[3].Id),
            (liege.Id,    lge[2].Id),
        };

        var organizerRotation = new[] { alice, bob, claire, david, emma, francois,
                                        helene, kevin, nathalie, ibrahim, lea, julie };

        // ~80 completed matches spread over 180 days
        var pastDays = new List<int>();
        for (var d = -180; d <= -2; d += 3)      // every 3 days base
            pastDays.Add(d);
        for (var d = -45; d <= -2; d += 2)        // denser last 45 days
        {
            if (!pastDays.Contains(d))
                pastDays.Add(d);
        }
        for (var d = -14; d <= -2; d++)           // daily last 2 weeks
        {
            if (!pastDays.Contains(d))
                pastDays.Add(d);
        }
        pastDays.Sort();

        // Some past matches are private (variety)
        for (var i = 0; i < pastDays.Count; i++)
        {
            var day = pastDays[i];
            var (siteId, courtId) = courtRotation[i % courtRotation.Length];
            var organizer = organizerRotation[i % organizerRotation.Length];

            var others = allPlayers
                .Where(p => p.Id != organizer.Id)
                .OrderBy(_ => (i * 31 + day * 7).GetHashCode())
                .Take(3)
                .Select(p => p.Id)
                .ToArray();

            // Every 5th match is a private completed match
            if (i % 5 == 0)
                SeedFullPrivateMatch(context, siteId, courtId,
                    organizer.Id, others, MatchDay(day));
            else
                SeedFullMatch(context, siteId, courtId,
                    organizer.Id, others, MatchDay(day));
        }

        // Add some double-header days (2 matches same day, different times/courts)
        for (var d = -60; d <= -5; d += 7)
        {
            var ci = ((-d) / 7) % courtRotation.Length;
            var (siteId, courtId) = courtRotation[(ci + 3) % courtRotation.Length];
            var organizer = organizerRotation[((-d) / 7 + 5) % organizerRotation.Length];

            var others = allPlayers
                .Where(p => p.Id != organizer.Id)
                .OrderBy(_ => (d * 13).GetHashCode())
                .Take(3)
                .Select(p => p.Id)
                .ToArray();

            SeedFullMatch(context, siteId, courtId,
                organizer.Id, others, MatchDay(d, 17)); // evening session
        }

        // ── TODAY's matches ──────────────────────────────────────────────

        // [BXL C1] Today 07:00 — completed this morning, 4 players
        SeedFullMatch(context, brussels.Id, bxl[0].Id,
            alice.Id, [bob.Id, claire.Id, david.Id], MatchDay(0, 7));

        // [BXL C2] Today 09:00 — completed, different players
        SeedFullMatch(context, brussels.Id, bxl[1].Id,
            kevin.Id, [helene.Id, nathalie.Id, emma.Id], MatchDay(0, 9));

        // [LGE C1] Today 08:00 — completed
        SeedFullMatch(context, liege.Id, lge[0].Id,
            francois.Id, [lea.Id, julie.Id, alice.Id], MatchDay(0, 8));

        // [LGE C2] Today 10:00 — completed
        SeedFullMatch(context, liege.Id, lge[1].Id,
            ibrahim.Id, [bob.Id, marc.Id, david.Id], MatchDay(0, 10));

        // ================================================================
        // INCOMPLETE MATCHES — Georges = blocked debtor (3 × 15 € = 45 €)
        // ================================================================

        // Match 1: Georges org, only 2 paid → 1 unpaid excluded → 15 € debt
        SeedIncompleteMatch(context, brussels.Id, bxl[2].Id,
            organizerId:  georges.Id,
            paidPlayers:  [alice.Id, bob.Id],
            unpaidPlayer: julie.Id,
            startAt: MatchDay(-30, 14));

        // Match 2: Georges org, only 2 paid → 1 unpaid excluded → 15 € debt
        SeedIncompleteMatch(context, brussels.Id, bxl[0].Id,
            organizerId:  georges.Id,
            paidPlayers:  [claire.Id, david.Id],
            unpaidPlayer: marc.Id,
            startAt: MatchDay(-18, 14));

        // Match 3: Georges org, only 2 paid → 1 unpaid excluded → 15 € debt
        SeedIncompleteMatch(context, liege.Id, lge[0].Id,
            organizerId:  georges.Id,
            paidPlayers:  [helene.Id, kevin.Id],
            unpaidPlayer: julie.Id,
            startAt: MatchDay(-8, 14));

        // Total debt: 3 × 1500 = 4500 cents (45 €) — Georges CANNOT create matches
        var georgesDebt = OrganizerDebt.Create(georges.Id, 1500, MatchDay(-30, 14));
        georgesDebt.IncreaseDebt(1500, MatchDay(-18, 14));
        georgesDebt.IncreaseDebt(1500, MatchDay(-8, 14));
        context.OrganizerDebts.Add(georgesDebt);

        // Bob also had one incomplete match — 15 € debt (lighter case)
        SeedIncompleteMatch(context, liege.Id, lge[1].Id,
            organizerId: bob.Id,
            paidPlayers: [alice.Id, francois.Id],
            unpaidPlayer: marc.Id,
            startAt: MatchDay(-12, 16));

        context.OrganizerDebts.Add(OrganizerDebt.Create(bob.Id, 1500, MatchDay(-12, 16)));

        // ── CANCELLED matches ────────────────────────────────────────────

        // Cancelled yesterday — Georges tried to organize but was cancelled
        SeedCancelledMatch(context, brussels.Id, bxl[1].Id,
            georges.Id, PadMatchType.Public, MatchDay(-1, 19));

        // Cancelled 5 days ago — not enough players
        SeedCancelledMatch(context, liege.Id, lge[2].Id,
            marc.Id, PadMatchType.Private, MatchDay(-5, 19));

        // Cancelled 15 days ago
        SeedCancelledMatch(context, brussels.Id, bxl[3].Id,
            julie.Id, PadMatchType.Public, MatchDay(-15, 19));

        // ================================================================
        // UPCOMING MATCHES — overview alerts + demo scenarios
        // ================================================================

        // ─── TOMORROW (J-1) — triggers J-1 alerts ───────────────────────

        // [BXL C1] +1d — PRIVATE, David org + Alice → 2/4, UNPAID → J-1 alert
        {
            var start = MatchDay(1, 9);
            var creation = now.AddHours(-6);
            var m = Match.Create(brussels.Id, bxl[0].Id, david.Id,
                start, start.AddMinutes(90), PadMatchType.Private, creation).Value;
            m.AddParticipant(alice.Id, creation);
            context.Matches.Add(m);
        }

        // [BXL C2] +1d — PUBLIC, Claire org + Georges + Julie → 3/4, UNPAID → J-1 alert
        {
            var start = MatchDay(1, 10);
            var creation = now.AddHours(-8);
            var m = Match.Create(brussels.Id, bxl[1].Id, claire.Id,
                start, start.AddMinutes(90), PadMatchType.Public, creation).Value;
            m.JoinPublic(georges.Id, creation);
            m.JoinPublic(julie.Id, creation);
            context.Matches.Add(m);
        }

        // [LGE C1] +1d — PRIVATE, François org + Léa → 2/4, UNPAID → J-1 alert
        {
            var start = MatchDay(1, 11);
            var creation = now.AddHours(-5);
            var m = Match.Create(liege.Id, lge[0].Id, francois.Id,
                start, start.AddMinutes(90), PadMatchType.Private, creation).Value;
            m.AddParticipant(lea.Id, creation);
            context.Matches.Add(m);
        }

        // [LGE C2] +1d — PUBLIC, Léa org, ALL 4 PAID → Full, no alert
        {
            var start = MatchDay(1, 14);
            var creation = now.AddHours(-12);
            var m = Match.Create(liege.Id, lge[1].Id, lea.Id,
                start, start.AddMinutes(90), PadMatchType.Public, creation).Value;
            var r2 = m.JoinPublic(alice.Id,  creation).Value;
            var r3 = m.JoinPublic(bob.Id,    creation).Value;
            var r4 = m.JoinPublic(nathalie.Id, creation).Value;

            var org = m.Participants.First(p => p.Role == ParticipantRole.Organizer);
            m.ConfirmPayment(org.Id, creation);
            m.ConfirmPayment(r2.Id,  creation);
            m.ConfirmPayment(r3.Id,  creation);
            m.ConfirmPayment(r4.Id,  creation);

            foreach (var p in m.Participants)
                AddPaidPayment(context, m.Id, p.MemberId, p.Id, creation);

            context.Matches.Add(m);
        }

        // ─── J+2 to J+5 — various upcoming scenarios ────────────────────

        // [BXL C3] +2d — PUBLIC, Kevin org + Hélène joined (paid), Ibrahim joined (unpaid)
        {
            var start = MatchDay(2, 18);
            var creation = now.AddHours(-4);
            var m = Match.Create(brussels.Id, bxl[2].Id, kevin.Id,
                start, start.AddMinutes(90), PadMatchType.Public, creation).Value;
            var r2 = m.JoinPublic(helene.Id, creation).Value;
            m.JoinPublic(ibrahim.Id, creation);

            var org = m.Participants.First(p => p.Role == ParticipantRole.Organizer);
            m.ConfirmPayment(org.Id, creation);
            m.ConfirmPayment(r2.Id,  creation);
            AddPaidPayment(context, m.Id, kevin.Id, org.Id, creation);
            AddPaidPayment(context, m.Id, helene.Id, r2.Id, creation);

            context.Matches.Add(m);
        }

        // [BXL C4] +3d — PRIVATE, Alice org + Bob + Claire → 3/4, all unpaid
        {
            var start = MatchDay(3, 9);
            var creation = now.AddHours(-3);
            var m = Match.Create(brussels.Id, bxl[3].Id, alice.Id,
                start, start.AddMinutes(90), PadMatchType.Private, creation).Value;
            m.AddParticipant(bob.Id, creation);
            m.AddParticipant(claire.Id, creation);
            context.Matches.Add(m);
        }

        // [LGE C1] +3d — PUBLIC, François org, only organizer → 1/4 open
        {
            var start = MatchDay(3, 10);
            var creation = now.AddHours(-2);
            var m = Match.Create(liege.Id, lge[0].Id, francois.Id,
                start, start.AddMinutes(90), PadMatchType.Public, creation).Value;
            context.Matches.Add(m);
        }

        // [BXL C1] +4d — PUBLIC, Nathalie org + David (paid) + Marc (unpaid) → 3/4
        {
            var start = MatchDay(4, 19);
            var creation = now.AddHours(-6);
            var m = Match.Create(brussels.Id, bxl[0].Id, nathalie.Id,
                start, start.AddMinutes(90), PadMatchType.Public, creation).Value;
            var r2 = m.JoinPublic(david.Id,  creation).Value;
            m.JoinPublic(marc.Id, creation);

            var org = m.Participants.First(p => p.Role == ParticipantRole.Organizer);
            m.ConfirmPayment(org.Id, creation);
            m.ConfirmPayment(r2.Id,  creation);
            AddPaidPayment(context, m.Id, nathalie.Id, org.Id, creation);
            AddPaidPayment(context, m.Id, david.Id, r2.Id, creation);

            context.Matches.Add(m);
        }

        // [LGE C2] +5d — PUBLIC, ALL 4 PAID + full → perfect match
        {
            var start = MatchDay(5, 14);
            var creation = now.AddHours(-10);
            var m = Match.Create(liege.Id, lge[1].Id, francois.Id,
                start, start.AddMinutes(90), PadMatchType.Public, creation).Value;
            var r2 = m.JoinPublic(alice.Id,  creation).Value;
            var r3 = m.JoinPublic(kevin.Id,  creation).Value;
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

        // ─── J+6 to J+21 — longer-term upcoming ─────────────────────────

        // [BXL C2] +7d — PRIVATE, Emma org + Ibrahim → 2/4 unpaid
        {
            var start = MatchDay(7, 18);
            var creation = now.AddHours(-2);
            var m = Match.Create(brussels.Id, bxl[1].Id, emma.Id,
                start, start.AddMinutes(90), PadMatchType.Private, creation).Value;
            m.AddParticipant(ibrahim.Id, creation);
            context.Matches.Add(m);
        }

        // [LGE C1] +8d — PUBLIC, Léa org + Julie joined → 2/4 open
        {
            var start = MatchDay(8, 10);
            var creation = now.AddHours(-1);
            var m = Match.Create(liege.Id, lge[0].Id, lea.Id,
                start, start.AddMinutes(90), PadMatchType.Public, creation).Value;
            m.JoinPublic(julie.Id, creation);
            context.Matches.Add(m);
        }

        // [BXL C3] +10d — PUBLIC, Hélène org → 1/4 just created
        {
            var start = MatchDay(10, 9);
            var creation = now.AddMinutes(-30);
            var m = Match.Create(brussels.Id, bxl[2].Id, helene.Id,
                start, start.AddMinutes(90), PadMatchType.Public, creation).Value;
            context.Matches.Add(m);
        }

        // [BXL C4] +14d — PUBLIC, David org + Marc joined → 2/4 open
        {
            var start = MatchDay(14, 19);
            var creation = now.AddMinutes(-45);
            var m = Match.Create(brussels.Id, bxl[3].Id, david.Id,
                start, start.AddMinutes(90), PadMatchType.Public, creation).Value;
            m.JoinPublic(marc.Id, creation);
            context.Matches.Add(m);
        }

        // [LGE C2] +18d — PRIVATE, Ibrahim org + Alice + Bob → 3/4
        {
            var start = MatchDay(18, 17);
            var creation = now.AddMinutes(-20);
            var m = Match.Create(liege.Id, lge[1].Id, ibrahim.Id,
                start, start.AddMinutes(90), PadMatchType.Private, creation).Value;
            m.AddParticipant(alice.Id, creation);
            m.AddParticipant(bob.Id, creation);
            context.Matches.Add(m);
        }

        // [BXL C1] +21d — PUBLIC, Nathalie org → 1/4 far future
        {
            var start = MatchDay(21, 10);
            var creation = now.AddMinutes(-10);
            var m = Match.Create(brussels.Id, bxl[0].Id, nathalie.Id,
                start, start.AddMinutes(90), PadMatchType.Public, creation).Value;
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

    private static void SeedFullPrivateMatch(
        PadTimeDbContext context,
        Guid siteId, Guid courtId,
        Guid organizerId, Guid[] players,
        DateTime startAt)
    {
        var creation = startAt.AddHours(-10);

        var m = Match.Create(siteId, courtId, organizerId,
            startAt, startAt.AddMinutes(90),
            PadMatchType.Private, creation).Value;

        foreach (var pid in players)
            m.AddParticipant(pid, creation);

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

    private static void SeedCancelledMatch(
        PadTimeDbContext context,
        Guid siteId, Guid courtId,
        Guid organizerId, PadMatchType type,
        DateTime startAt)
    {
        var creation = startAt.AddHours(-12);
        var m = Match.Create(siteId, courtId, organizerId,
            startAt, startAt.AddMinutes(90), type, creation).Value;
        m.Cancel(startAt.AddHours(-6));
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
