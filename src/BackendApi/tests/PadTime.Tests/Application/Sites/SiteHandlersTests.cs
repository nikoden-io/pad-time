using FluentAssertions;
using NSubstitute;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Application.Common.Models;
using PadTime.Application.Sites.Commands.ActivateSite;
using PadTime.Application.Sites.Commands.CreateCourt;
using PadTime.Application.Sites.Commands.CreateSite;
using PadTime.Application.Sites.Queries.GetCourtById;
using PadTime.Application.Sites.Queries.GetCourts;
using PadTime.Application.Sites.Queries.GetSiteById;
using PadTime.Application.Sites.Queries.GetSiteStatistics;
using PadTime.Application.Sites.Queries.GetSites;
using PadTime.Domain.Site;
using PadTime.Tests.TestSupport;
using Xunit;

namespace PadTime.Tests.Application.Sites;

public sealed class SiteHandlersTests
{
    [Fact]
    public async Task CreateSite_WhenCalled_PersistsSiteAndReturnsId()
    {
        var sites = Substitute.For<ISiteRepository>();
        var uow = Substitute.For<IUnitOfWork>();
        var clock = Substitute.For<IDateTimeProvider>();
        clock.UtcNow.Returns(new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc));
        var handler = new CreateSiteCommandHandler(sites, uow, clock);

        var result = await handler.Handle(new CreateSiteCommand("Main", "1", "Street", "1000", "Brussels", "Belgium", "UTC"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await sites.Received(1).AddAsync(Arg.Is<Site>(s => s.Name == "Main"), Arg.Any<CancellationToken>());
        await uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCourt_WhenDuplicateLabelExists_ReturnsDuplicateLabel()
    {
        var siteRepository = Substitute.For<ISiteRepository>();
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var site = Site.Create("Main", "1", "Street", "1000", "Brussels", "Belgium", "UTC", DateTime.UtcNow);
        site.AddCourt("Court 1", DateTime.UtcNow);
        siteRepository.GetByIdAsync(site.Id, Arg.Any<CancellationToken>()).Returns(site);
        var handler = new CreateCourtCommandHandler(siteRepository, dateTimeProvider, unitOfWork);

        var result = await handler.Handle(new CreateCourtCommand(site.Id, "Court 1"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Should().Be(DomainErrors.Court.DuplicateLabel);
    }

    [Fact]
    public async Task ActivateSite_WhenSiteIsInactive_ActivatesAndSaves()
    {
        var site = Site.Create("Main", "1", "Street", "1000", "Brussels", "Belgium", "UTC", DateTime.UtcNow);
        site.Deactivate(DateTime.UtcNow);
        var sites = Substitute.For<ISiteRepository>();
        var uow = Substitute.For<IUnitOfWork>();
        var clock = Substitute.For<IDateTimeProvider>();
        sites.GetByIdAsync(site.Id, Arg.Any<CancellationToken>()).Returns(site);
        var handler = new ActivateSiteCommandHandler(sites, uow, clock);

        var result = await handler.Handle(new ActivateSiteCommand(site.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        site.IsActive.Should().BeTrue();
        await sites.Received(1).UpdateAsync(site, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCourtById_WhenCourtExists_ReturnsMappedCourt()
    {
        var site = Site.Create("Main", "1", "Street", "1000", "Brussels", "Belgium", "UTC", DateTime.UtcNow);
        var court = site.AddCourt("Court 1", DateTime.UtcNow);
        var courtRepository = Substitute.For<ICourtRepository>();
        courtRepository.GetByIdAsync(court.Id, Arg.Any<CancellationToken>()).Returns(court);
        var handler = new GetCourtByIdQueryHandler(courtRepository);

        var result = await handler.Handle(new GetCourtByIdQuery(court.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Label.Should().Be("Court 1");
    }

    [Fact]
    public async Task GetCourts_WhenSiteExists_ReturnsOrderedCourts()
    {
        var site = Site.Create("Main", "1", "Street", "1000", "Brussels", "Belgium", "UTC", DateTime.UtcNow);
        site.AddCourt("Court B", DateTime.UtcNow);
        site.AddCourt("Court A", DateTime.UtcNow);
        var siteRepository = Substitute.For<ISiteRepository>();
        siteRepository.GetByIdAsync(site.Id, Arg.Any<CancellationToken>()).Returns(site);
        var handler = new GetCourtsQueryHandler(siteRepository);

        var result = await handler.Handle(new GetCourtsQuery(site.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(c => c.Label).Should().Equal("Court A", "Court B");
    }

    [Fact]
    public async Task GetSiteById_WhenSiteExists_ReturnsDetailedDto()
    {
        var site = Site.Create("Main", "1", "Street", "1000", "Brussels", "Belgium", "UTC", DateTime.UtcNow);
        site.AddCourt("Court 1", DateTime.UtcNow);
        site.AddSchedule("Regular", new DateOnly(2026, 1, 1), null, new TimeOnly(8, 0), new TimeOnly(22, 0), null, 1, DateTime.UtcNow);
        site.AddFullDayClosure(new DateOnly(2026, 4, 10), ClosureReason.Maintenance, "Closed", null, DateTime.UtcNow);
        var siteRepository = Substitute.For<ISiteRepository>();
        siteRepository.GetByIdWithSchedulesAndClosuresAsync(site.Id, Arg.Any<CancellationToken>()).Returns(site);
        var handler = new GetSiteByIdQueryHandler(siteRepository);

        var result = await handler.Handle(new GetSiteByIdQuery(site.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Courts.Should().ContainSingle();
        result.Value.Schedules.Should().ContainSingle();
        result.Value.Closures.Should().ContainSingle();
    }

    [Fact]
    public async Task GetSites_WhenPagedSitesReturned_MapsCourtCount()
    {
        var site = Site.Create("Main", "1", "Street", "1000", "Brussels", "Belgium", "UTC", DateTime.UtcNow);
        site.SetEntityId(Guid.NewGuid());
        site.AddCourt("Court 1", DateTime.UtcNow);
        var siteRepository = Substitute.For<ISiteRepository>();
        siteRepository.GetPagedAsync(1, 20, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Site>([site], 1, 20, 1));
        var handler = new GetSitesQueryHandler(siteRepository);

        var result = await handler.Handle(new GetSitesQuery(), CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].CourtCount.Should().Be(1);
    }

    [Fact]
    public async Task GetSiteStatistics_WhenSiteExists_ReturnsCalculatedStatistics()
    {
        var site = Site.Create("Main", "1", "Street", "1000", "Brussels", "Belgium", "UTC", DateTime.UtcNow);
        site.AddCourt("Court 1", DateTime.UtcNow);
        var siteRepository = Substitute.For<ISiteRepository>();
        var statisticsRepository = Substitute.For<ISiteStatisticsRepository>();
        siteRepository.GetByIdAsync(site.Id, Arg.Any<CancellationToken>()).Returns(site);
        statisticsRepository.GetBookingCountForPeriodAsync(site.Id, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(10, 5);
        statisticsRepository.GetUpcomingBookingCountAsync(site.Id, Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>()).Returns(2, 8);
        statisticsRepository.GetLastBookingDateAsync(site.Id, Arg.Any<CancellationToken>()).Returns(DateTime.UtcNow.AddDays(-1));
        statisticsRepository.GetCourtUtilizationAsync(site.Id, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([new CourtUtilizationStats(site.Courts[0].Id, "Court 1", 10, 55.55m)]);
        statisticsRepository.GetDailyBookingStatsAsync(site.Id, Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns([new DailyBookingStats(new DateOnly(2026, 4, 1), 3, 2)]);
        var handler = new GetSiteStatisticsQueryHandler(siteRepository, statisticsRepository);

        var result = await handler.Handle(new GetSiteStatisticsQuery(site.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalBookingsThisMonth.Should().Be(10);
        result.Value.TotalBookingsLastMonth.Should().Be(5);
        result.Value.CourtUtilization.Should().ContainSingle();
    }
}
