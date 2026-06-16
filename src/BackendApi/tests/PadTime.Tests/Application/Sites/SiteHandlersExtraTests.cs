using FluentAssertions;
using NSubstitute;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Application.Sites.Commands.AddSiteClosure;
using PadTime.Application.Sites.Commands.CreateSiteSchedule;
using PadTime.Application.Sites.Commands.DeactivateSite;
using PadTime.Application.Sites.Commands.DeleteCourt;
using PadTime.Application.Sites.Commands.DeleteSite;
using PadTime.Application.Sites.Commands.DeleteSiteSchedule;
using PadTime.Application.Sites.Commands.RemoveSiteClosure;
using PadTime.Application.Sites.Commands.UpdateCourt;
using PadTime.Application.Sites.Commands.UpdateSite;
using PadTime.Application.Sites.Queries.GetCourtById;
using PadTime.Application.Sites.Queries.GetCourts;
using PadTime.Application.Sites.Queries.GetSiteById;
using PadTime.Application.Sites.Queries.GetSiteSchedule;
using PadTime.Application.Sites.Queries.GetSiteStatistics;
using PadTime.Domain.Common;
using PadTime.Domain.Site;
using Xunit;

namespace PadTime.Tests.Application.Sites;

public sealed class SiteHandlersExtraTests
{
    private static readonly DateTime Clock = new(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);

    private static Site NewSite()
        => Site.Create("Main", "1", "Street", "1000", "Brussels", "Belgium", "UTC", Clock);

    private static ISiteRepository Sites() => Substitute.For<ISiteRepository>();
    private static IUnitOfWork Uow() => Substitute.For<IUnitOfWork>();

    private static IDateTimeProvider ClockSub()
    {
        var c = Substitute.For<IDateTimeProvider>();
        c.UtcNow.Returns(Clock);
        return c;
    }

    private static void StubSite(ISiteRepository sites, Site site)
    {
        sites.GetByIdAsync(site.Id, Arg.Any<CancellationToken>()).Returns(site);
        sites.GetByIdWithSchedulesAndClosuresAsync(site.Id, Arg.Any<CancellationToken>()).Returns(site);
    }

    // =================== NOT-FOUND GUARDS ===================
    // Unstubbed repositories return null, so every handler hits its "site/court not found" guard.

    [Fact]
    public async Task DeactivateSite_WhenSiteMissing_ReturnsNotFound()
    {
        var result = await new DeactivateSiteCommandHandler(Sites(), Uow(), ClockSub())
            .Handle(new DeactivateSiteCommand(Guid.NewGuid()), CancellationToken.None);
        result.PadTimeError.Should().Be(DomainErrors.Site.NotFound);
    }

    [Fact]
    public async Task DeleteSite_WhenSiteMissing_ReturnsNotFound()
    {
        var result = await new DeleteSiteCommandHandler(Sites(), Uow())
            .Handle(new DeleteSiteCommand(Guid.NewGuid()), CancellationToken.None);
        result.PadTimeError.Should().Be(DomainErrors.Site.NotFound);
    }

    [Fact]
    public async Task DeleteCourt_WhenSiteMissing_ReturnsNotFound()
    {
        var result = await new DeleteCourtCommandHandler(Sites(), Substitute.For<IMatchRepository>(), ClockSub(), Uow())
            .Handle(new DeleteCourtCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);
        result.PadTimeError.Should().Be(DomainErrors.Site.NotFound);
    }

    [Fact]
    public async Task UpdateSite_WhenSiteMissing_ReturnsNotFound()
    {
        var result = await new UpdateSiteCommandHandler(Sites(), Uow(), ClockSub())
            .Handle(new UpdateSiteCommand(Guid.NewGuid(), "N", "1", "S", "1000", "C", "Co", "UTC"), CancellationToken.None);
        result.PadTimeError.Should().Be(DomainErrors.Site.NotFound);
    }

    [Fact]
    public async Task UpdateCourt_WhenSiteMissing_ReturnsNotFound()
    {
        var result = await new UpdateCourtCommandHandler(Sites(), ClockSub(), Uow())
            .Handle(new UpdateCourtCommand(Guid.NewGuid(), Guid.NewGuid(), "L"), CancellationToken.None);
        result.PadTimeError.Should().Be(DomainErrors.Site.NotFound);
    }

    [Fact]
    public async Task CreateSiteSchedule_WhenSiteMissing_ReturnsNotFound()
    {
        var result = await new CreateSiteScheduleCommandHandler(Sites(), Uow(), ClockSub())
            .Handle(new CreateSiteScheduleCommand(Guid.NewGuid(), "Default",
                DateOnly.FromDateTime(Clock), null, new TimeOnly(8, 0), new TimeOnly(22, 0), null, 0),
                CancellationToken.None);
        result.PadTimeError.Should().Be(DomainErrors.Site.NotFound);
    }

    [Fact]
    public async Task AddSiteClosure_WhenSiteMissing_ReturnsNotFound()
    {
        var result = await new AddSiteClosureCommandHandler(Sites(), Uow(), ClockSub())
            .Handle(new AddSiteClosureCommand(Guid.NewGuid(), ClosureType.FullDay, ClosureReason.PublicHoliday,
                null, DateOnly.FromDateTime(Clock).AddDays(7), null, null, null, null), CancellationToken.None);
        result.PadTimeError.Should().Be(DomainErrors.Site.NotFound);
    }

    [Fact]
    public async Task DeleteSiteSchedule_WhenSiteMissing_ReturnsNotFound()
    {
        var result = await new DeleteSiteScheduleCommandHandler(Sites(), ClockSub(), Uow())
            .Handle(new DeleteSiteScheduleCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);
        result.PadTimeError.Should().Be(DomainErrors.Site.NotFound);
    }

    [Fact]
    public async Task RemoveSiteClosure_WhenSiteMissing_ReturnsNotFound()
    {
        var result = await new RemoveSiteClosureCommandHandler(Sites(), Uow(), ClockSub())
            .Handle(new RemoveSiteClosureCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);
        result.PadTimeError.Should().Be(DomainErrors.Site.NotFound);
    }

    [Fact]
    public async Task GetSiteById_WhenSiteMissing_ReturnsNotFound()
    {
        var result = await new GetSiteByIdQueryHandler(Sites())
            .Handle(new GetSiteByIdQuery(Guid.NewGuid()), CancellationToken.None);
        result.PadTimeError.Should().Be(DomainErrors.Site.NotFound);
    }

    [Fact]
    public async Task GetSiteSchedule_WhenSiteMissing_ReturnsNotFound()
    {
        var result = await new GetSiteScheduleQueryHandler(Sites())
            .Handle(new GetSiteScheduleQuery(Guid.NewGuid()), CancellationToken.None);
        result.PadTimeError.Should().Be(DomainErrors.Site.NotFound);
    }

    [Fact]
    public async Task GetSiteStatistics_WhenSiteMissing_ReturnsNotFound()
    {
        var result = await new GetSiteStatisticsQueryHandler(Sites(), Substitute.For<ISiteStatisticsRepository>())
            .Handle(new GetSiteStatisticsQuery(Guid.NewGuid()), CancellationToken.None);
        result.PadTimeError.Should().Be(DomainErrors.Site.NotFound);
    }

    [Fact]
    public async Task GetCourts_WhenSiteMissing_ReturnsNotFound()
    {
        var result = await new GetCourtsQueryHandler(Sites())
            .Handle(new GetCourtsQuery(Guid.NewGuid()), CancellationToken.None);
        result.PadTimeError.Should().Be(DomainErrors.Site.NotFound);
    }

    [Fact]
    public async Task GetCourtById_WhenCourtMissing_ReturnsNotFound()
    {
        var result = await new GetCourtByIdQueryHandler(Substitute.For<ICourtRepository>())
            .Handle(new GetCourtByIdQuery(Guid.NewGuid()), CancellationToken.None);
        result.PadTimeError.Should().Be(DomainErrors.Court.NotFound);
    }

    // =================== HAPPY / EDGE PATHS ===================

    [Fact]
    public async Task DeactivateSite_WhenActive_DeactivatesAndSaves()
    {
        var site = NewSite();
        var sites = Sites();
        var uow = Uow();
        StubSite(sites, site);
        var result = await new DeactivateSiteCommandHandler(sites, uow, ClockSub())
            .Handle(new DeactivateSiteCommand(site.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        site.IsActive.Should().BeFalse();
        await uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivateSite_WhenAlreadyInactive_ReturnsAlreadyDeactivated()
    {
        var site = NewSite();
        site.Deactivate(Clock);
        var sites = Sites();
        StubSite(sites, site);
        var result = await new DeactivateSiteCommandHandler(sites, Uow(), ClockSub())
            .Handle(new DeactivateSiteCommand(site.Id), CancellationToken.None);

        result.PadTimeError.Should().Be(DomainErrors.Site.SiteAlreadyDeactivated);
    }

    [Fact]
    public async Task DeleteCourt_WhenCourtMissing_ReturnsCourtNotFound()
    {
        var site = NewSite();
        var sites = Sites();
        StubSite(sites, site);
        var result = await new DeleteCourtCommandHandler(sites, Substitute.For<IMatchRepository>(), ClockSub(), Uow())
            .Handle(new DeleteCourtCommand(site.Id, Guid.NewGuid()), CancellationToken.None);

        result.PadTimeError.Should().Be(DomainErrors.Court.NotFound);
    }

    [Fact]
    public async Task DeleteCourt_WhenCourtHasActiveBookings_ReturnsCannotDelete()
    {
        var site = NewSite();
        var court = site.AddCourt("Court 1", Clock);
        var sites = Sites();
        StubSite(sites, site);
        var matches = Substitute.For<IMatchRepository>();
        matches.HasActiveBookingsForCourtAsync(court.Id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await new DeleteCourtCommandHandler(sites, matches, ClockSub(), Uow())
            .Handle(new DeleteCourtCommand(site.Id, court.Id), CancellationToken.None);

        result.PadTimeError.Should().Be(DomainErrors.Court.CannotDeleteWithActiveBookings);
    }

    [Fact]
    public async Task DeleteCourt_WhenNoActiveBookings_Succeeds()
    {
        var site = NewSite();
        var court = site.AddCourt("Court 1", Clock);
        var sites = Sites();
        var uow = Uow();
        StubSite(sites, site);
        var matches = Substitute.For<IMatchRepository>();
        matches.HasActiveBookingsForCourtAsync(court.Id, Arg.Any<CancellationToken>()).Returns(false);

        var result = await new DeleteCourtCommandHandler(sites, matches, ClockSub(), uow)
            .Handle(new DeleteCourtCommand(site.Id, court.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCourt_WhenCourtExists_Succeeds()
    {
        var site = NewSite();
        var court = site.AddCourt("Court 1", Clock);
        var sites = Sites();
        var uow = Uow();
        StubSite(sites, site);

        var result = await new UpdateCourtCommandHandler(sites, ClockSub(), uow)
            .Handle(new UpdateCourtCommand(site.Id, court.Id, "Center Court"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        site.Courts.Should().ContainSingle().Which.Label.Should().Be("Center Court");
        await uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateSiteSchedule_WhenValid_AddsScheduleAndSaves()
    {
        var site = NewSite();
        var sites = Sites();
        var uow = Uow();
        StubSite(sites, site);

        var result = await new CreateSiteScheduleCommandHandler(sites, uow, ClockSub())
            .Handle(new CreateSiteScheduleCommand(site.Id, "Default",
                DateOnly.FromDateTime(Clock), null, new TimeOnly(8, 0), new TimeOnly(22, 0), null, 0),
                CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddSiteClosure_WhenFullDay_AddsClosureAndSaves()
    {
        var site = NewSite();
        var sites = Sites();
        var uow = Uow();
        StubSite(sites, site);

        var result = await new AddSiteClosureCommandHandler(sites, uow, ClockSub())
            .Handle(new AddSiteClosureCommand(site.Id, ClosureType.FullDay, ClosureReason.PublicHoliday,
                "New Year", DateOnly.FromDateTime(Clock).AddDays(30), null, null, null, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
