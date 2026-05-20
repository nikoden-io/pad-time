using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PadTime.Application.Admin.Queries.GetAiTrends;
using PadTime.Application.Admin.Queries.GetSiteOverview;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Billing;
using PadTime.Domain.Booking;
using PadTime.Domain.Site;
using Xunit;

namespace PadTime.Tests.Application.Admin;

public sealed class AdminAiAndOverviewTests
{
    [Fact]
    public async Task GetAiTrends_WhenAiReturnsNull_ReturnsFallbackResponse()
    {
        var siteRepository = Substitute.For<ISiteRepository>();
        var statisticsRepository = Substitute.For<ISiteStatisticsRepository>();
        var paymentRepository = Substitute.For<IPaymentRepository>();
        var memberRepository = Substitute.For<IMemberRepository>();
        var currentUser = Substitute.For<ICurrentUser>();
        var aiService = Substitute.For<IAiCompletionService>();
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        var logger = Substitute.For<ILogger<GetAiTrendsQueryHandler>>();
        var now = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
        var site = Site.Create("Main", "1", "Street", "1000", "Brussels", "Belgium", "UTC", now);

        dateTimeProvider.UtcNow.Returns(now);
        siteRepository.GetAllActiveAsync(Arg.Any<CancellationToken>()).Returns([site]);
        statisticsRepository.GetBookingCountForPeriodAsync(site.Id, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(4, 2);
        statisticsRepository.GetCourtUtilizationAsync(site.Id, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns([]);
        paymentRepository.GetPaidBySiteAndDateRangeAsync(null, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns([]);
        memberRepository.GetPagedAsync(1, 1, null, null, null, Arg.Any<CancellationToken>()).Returns((new List<PadTime.Domain.Members.Member>(), 0));
        aiService.CompleteJsonAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((string?)null);

        var handler = new GetAiTrendsQueryHandler(siteRepository, statisticsRepository, paymentRepository, memberRepository, currentUser, aiService, dateTimeProvider, logger);

        var result = await handler.Handle(new GetAiTrendsQuery(null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.FallbackUsed.Should().BeTrue();
        result.Value.Trends.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSiteOverview_WhenSiteAdminTargetsOtherSite_ReturnsNotFound()
    {
        var matchRepository = Substitute.For<IMatchRepository>();
        var debtRepository = Substitute.For<IOrganizerDebtRepository>();
        var currentUser = Substitute.For<ICurrentUser>();
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        var targetSiteId = Guid.NewGuid();

        currentUser.IsSiteAdmin.Returns(true);
        currentUser.IsGlobalAdmin.Returns(false);
        currentUser.SiteId.Returns(Guid.NewGuid());

        var handler = new GetSiteOverviewQueryHandler(matchRepository, debtRepository, currentUser, dateTimeProvider);

        var result = await handler.Handle(new GetSiteOverviewQuery(targetSiteId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Should().Be(PadTime.Domain.Common.DomainErrors.Site.NotFound);
    }

    [Fact]
    public async Task GetSiteOverview_WhenMatchesAndDebtsExist_ReturnsAlerts()
    {
        var siteId = Guid.NewGuid();
        var now = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
        var privateMatch = Match.Create(siteId, Guid.NewGuid(), Guid.NewGuid(), now.AddDays(1), now.AddDays(1).AddMinutes(90), PadMatchType.Private, now).Value;
        var upcomingMatch = Match.Create(siteId, Guid.NewGuid(), Guid.NewGuid(), now.AddDays(2), now.AddDays(2).AddMinutes(90), PadMatchType.Private, now).Value;

        var matchRepository = Substitute.For<IMatchRepository>();
        var debtRepository = Substitute.For<IOrganizerDebtRepository>();
        var currentUser = Substitute.For<ICurrentUser>();
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();

        dateTimeProvider.UtcNow.Returns(now);
        matchRepository.GetMatchesForDayBeforeProcessingAsync(now.Date.AddDays(1), Arg.Any<CancellationToken>()).Returns([privateMatch]);
        matchRepository.GetBySiteIdAsync(siteId, now, now.AddDays(7), 1, 200, Arg.Any<CancellationToken>()).Returns([upcomingMatch]);
        debtRepository.GetAllActiveAsync(Arg.Any<CancellationToken>()).Returns([OrganizerDebt.Create(Guid.NewGuid(), 1500, now)]);

        var handler = new GetSiteOverviewQueryHandler(matchRepository, debtRepository, currentUser, dateTimeProvider);

        var result = await handler.Handle(new GetSiteOverviewQuery(siteId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Alerts.Should().HaveCount(3);
    }
}
