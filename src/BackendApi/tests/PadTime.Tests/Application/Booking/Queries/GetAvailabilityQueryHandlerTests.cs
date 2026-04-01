using FluentAssertions;
using NSubstitute;
using PadTime.Application.Booking.Queries.GetAvailability;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Site;
using PadTime.Tests.TestSupport;
using Xunit;

namespace PadTime.Tests.Application.Booking.Queries;

public sealed class GetAvailabilityQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenSiteIsClosed_ReturnsNoSlots()
    {
        var siteId = Guid.NewGuid();
        var site = CreateSite(siteId, closeSiteOn: new DateOnly(2026, 4, 10));
        var siteRepository = Substitute.For<ISiteRepository>();
        var courtRepository = Substitute.For<ICourtRepository>();
        var matchRepository = Substitute.For<IMatchRepository>();

        siteRepository.GetByIdWithSchedulesAndClosuresAsync(siteId, Arg.Any<CancellationToken>())
            .Returns(site);

        var handler = new GetAvailabilityQueryHandler(siteRepository, courtRepository, matchRepository);

        var result = await handler.Handle(new GetAvailabilityQuery(siteId, new DateOnly(2026, 4, 10)), CancellationToken.None);

        result.Slots.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenSingleCourtRequested_ReturnsOnlyThatCourtSlotsWithBookedFlags()
    {
        var siteId = Guid.NewGuid();
        var courtId = Guid.NewGuid();
        var site = CreateSite(siteId);
        var court = site.Courts.Single();
        court.SetEntityId(courtId);

        var siteRepository = Substitute.For<ISiteRepository>();
        var courtRepository = Substitute.For<ICourtRepository>();
        var matchRepository = Substitute.For<IMatchRepository>();

        siteRepository.GetByIdWithSchedulesAndClosuresAsync(siteId, Arg.Any<CancellationToken>())
            .Returns(site);
        courtRepository.GetByIdAsync(courtId, Arg.Any<CancellationToken>())
            .Returns(court);
        matchRepository.ExistsForSlotAsync(
                courtId,
                new DateTime(2026, 4, 10, 8, 0, 0, DateTimeKind.Utc),
                Arg.Any<CancellationToken>())
            .Returns(true);
        matchRepository.ExistsForSlotAsync(
                courtId,
                new DateTime(2026, 4, 10, 9, 45, 0, DateTimeKind.Utc),
                Arg.Any<CancellationToken>())
            .Returns(false);

        var handler = new GetAvailabilityQueryHandler(siteRepository, courtRepository, matchRepository);

        var result = await handler.Handle(new GetAvailabilityQuery(siteId, new DateOnly(2026, 4, 10), courtId), CancellationToken.None);

        result.Slots.Should().HaveCount(2);
        result.Slots.Should().ContainSingle(s => s.StartAtUtc == new DateTime(2026, 4, 10, 8, 0, 0, DateTimeKind.Utc) && !s.Available);
        result.Slots.Should().ContainSingle(s => s.StartAtUtc == new DateTime(2026, 4, 10, 9, 45, 0, DateTimeKind.Utc) && s.Available);
    }

    [Fact]
    public async Task Handle_WhenCourtIsInactive_FiltersItOut()
    {
        var siteId = Guid.NewGuid();
        var site = CreateSite(siteId);
        site.Courts[0].Deactivate();

        var siteRepository = Substitute.For<ISiteRepository>();
        var courtRepository = Substitute.For<ICourtRepository>();
        var matchRepository = Substitute.For<IMatchRepository>();

        siteRepository.GetByIdWithSchedulesAndClosuresAsync(siteId, Arg.Any<CancellationToken>())
            .Returns(site);
        courtRepository.GetBySiteIdAsync(siteId, Arg.Any<CancellationToken>())
            .Returns(site.Courts.ToList());

        var handler = new GetAvailabilityQueryHandler(siteRepository, courtRepository, matchRepository);

        var result = await handler.Handle(new GetAvailabilityQuery(siteId, new DateOnly(2026, 4, 10)), CancellationToken.None);

        result.Slots.Should().BeEmpty();
    }

    private static Site CreateSite(Guid siteId, DateOnly? closeSiteOn = null)
    {
        var site = Site.Create("Site", "1", "Street", "1000", "Brussels", "Belgium", "UTC", DateTime.UtcNow);
        site.SetEntityId(siteId);
        site.AddCourt("Court 1", DateTime.UtcNow);
        site.AddSchedule(
            "Morning",
            new DateOnly(2026, 1, 1),
            null,
            new TimeOnly(8, 0),
            new TimeOnly(11, 30),
            null,
            1,
            DateTime.UtcNow);

        if (closeSiteOn.HasValue)
        {
            site.AddFullDayClosure(closeSiteOn.Value, ClosureReason.Maintenance, "Closed", null, DateTime.UtcNow);
        }

        return site;
    }
}
