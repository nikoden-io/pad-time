using FluentAssertions;
using NSubstitute;
using PadTime.Application.Admin.Commands.ToggleMemberStatus;
using PadTime.Application.Admin.Queries.GetMemberDetail;
using PadTime.Application.Admin.Queries.GetMembers;
using PadTime.Application.Admin.Queries.GetRevenueAnalytics;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;
using PadTime.Domain.Billing;
using PadTime.Domain.Booking;
using PadTime.Domain.Members;
using PadTime.Domain.Site;
using PadTime.Tests.TestSupport;
using Xunit;

namespace PadTime.Tests.Application.Admin;

public sealed class AdminHandlersTests
{
    [Fact]
    public async Task ToggleMemberStatus_WhenMemberExists_DeactivatesMember()
    {
        var member = Member.Create("subject", "G1234", null, DateTime.UtcNow).Value;
        var members = Substitute.For<IMemberRepository>();
        var uow = Substitute.For<IUnitOfWork>();
        var clock = Substitute.For<IDateTimeProvider>();
        clock.UtcNow.Returns(new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc));
        members.GetByIdAsync(member.Id, Arg.Any<CancellationToken>()).Returns(member);
        var handler = new ToggleMemberStatusCommandHandler(members, uow, clock);

        var result = await handler.Handle(new ToggleMemberStatusCommand(member.Id, false), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        member.IsActive.Should().BeFalse();
        await uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMemberDetail_WhenMemberExists_ReturnsAggregatedDetails()
    {
        var member = Member.Create("subject", "S12345", Guid.NewGuid(), DateTime.UtcNow).Value;
        var members = Substitute.For<IMemberRepository>();
        var matches = Substitute.For<IMatchRepository>();
        var debts = Substitute.For<IOrganizerDebtRepository>();
        var sites = Substitute.For<ISiteRepository>();
        var handler = new GetMemberDetailQueryHandler(members, matches, debts, sites);
        var site = Site.Create("Main Site", "1", "Street", "1000", "Brussels", "Belgium", "UTC", DateTime.UtcNow);
        site.SetEntityId(member.SiteId!.Value);
        var organized = Match.Create(site.Id, Guid.NewGuid(), member.Id, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddMinutes(90), PadMatchType.Private, DateTime.UtcNow).Value;
        var played = Match.Create(site.Id, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(2).AddMinutes(90), PadMatchType.Public, DateTime.UtcNow).Value;
        played.JoinPublic(member.Id, DateTime.UtcNow).IsSuccess.Should().BeTrue();

        members.GetByIdAsync(member.Id, Arg.Any<CancellationToken>()).Returns(member);
        members.GetMatchCountAsync(member.Id, Arg.Any<CancellationToken>()).Returns(7);
        debts.GetByMemberIdAsync(member.Id, Arg.Any<CancellationToken>()).Returns(OrganizerDebt.Create(member.Id, 1500, DateTime.UtcNow));
        sites.GetByIdAsync(member.SiteId.Value, Arg.Any<CancellationToken>()).Returns(site);
        matches.GetByMemberIdAsync(member.Id, null, 1, 5, Arg.Any<CancellationToken>()).Returns([organized, played]);
        matches.GetByMemberIdAsync(member.Id, null, 1, int.MaxValue, Arg.Any<CancellationToken>()).Returns([organized, played]);

        var result = await handler.Handle(new GetMemberDetailQuery(member.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.SiteName.Should().Be("Main Site");
        result.Value.MatchCount.Should().Be(7);
        result.Value.TotalMatchesOrganized.Should().Be(1);
        result.Value.TotalMatchesPlayed.Should().Be(1);
    }

    [Fact]
    public async Task GetMembers_WhenMembersExist_MapsSiteNamesMatchCountsAndDebt()
    {
        var siteId = Guid.NewGuid();
        var member = Member.Create("subject", "S12345", siteId, DateTime.UtcNow).Value;
        var members = Substitute.For<IMemberRepository>();
        var debts = Substitute.For<IOrganizerDebtRepository>();
        var sites = Substitute.For<ISiteRepository>();
        var site = Site.Create("Main Site", "1", "Street", "1000", "Brussels", "Belgium", "UTC", DateTime.UtcNow);
        site.SetEntityId(siteId);
        var handler = new GetMembersQueryHandler(members, debts, sites);

        members.GetPagedAsync(1, 20, null, null, null, Arg.Any<CancellationToken>())
            .Returns((new List<Member> { member }, 1));
        members.GetMatchCountsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int> { [member.Id] = 4 });
        debts.GetAllActiveAsync(Arg.Any<CancellationToken>())
            .Returns([OrganizerDebt.Create(member.Id, 2000, DateTime.UtcNow)]);
        sites.GetByIdAsync(siteId, Arg.Any<CancellationToken>()).Returns(site);

        var result = await handler.Handle(new GetMembersQuery(1, 20), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
        result.Value.Items[0].SiteName.Should().Be("Main Site");
        result.Value.Items[0].MatchCount.Should().Be(4);
        result.Value.Items[0].DebtAmountCents.Should().Be(2000);
    }

    [Fact]
    public async Task GetRevenueAnalytics_WhenSiteAdminRequestsOtherSite_UsesCurrentUserSite()
    {
        var paymentRepository = Substitute.For<IPaymentRepository>();
        var currentUser = Substitute.For<ICurrentUser>();
        var handler = new GetRevenueAnalyticsQueryHandler(paymentRepository, currentUser);
        var adminSiteId = Guid.NewGuid();
        var otherSiteId = Guid.NewGuid();
        var payment = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1500, PaymentPurpose.MatchParticipation, "idem", DateTime.UtcNow).Value;

        currentUser.IsSiteAdmin.Returns(true);
        currentUser.IsGlobalAdmin.Returns(false);
        currentUser.SiteId.Returns(adminSiteId);
        paymentRepository.GetPaidBySiteAndDateRangeAsync(adminSiteId, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([(payment, adminSiteId)]);

        var result = await handler.Handle(new GetRevenueAnalyticsQuery(otherSiteId, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
        result.Value.Items[0].SiteId.Should().Be(adminSiteId);
    }
}
