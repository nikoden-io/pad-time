using FluentAssertions;
using NSubstitute;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Application.Sites.Queries.GetSiteSchedule;
using PadTime.Domain.Common;
using PadTime.Domain.Site;
using Xunit;

namespace PadTime.Application.Tests.Sites.Queries.GetSiteSchedule;

public class GetSiteScheduleQueryHandlerTests
{
    private readonly ISiteRepository _siteRepository;
    private readonly GetSiteScheduleQueryHandler _handler;

    public GetSiteScheduleQueryHandlerTests()
    {
        _siteRepository = Substitute.For<ISiteRepository>();
        _handler = new GetSiteScheduleQueryHandler(_siteRepository);
    }

    [Fact]
    public async Task Handle_SiteNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var query = new GetSiteScheduleQuery(Guid.NewGuid());
        _siteRepository.GetByIdWithSchedulesAndClosuresAsync(query.SiteId, Arg.Any<CancellationToken>())
            .Returns((Site?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Should().Be(DomainErrors.Site.NotFound);
    }

    [Fact]
    public async Task Handle_SiteExists_ReturnsScheduleDetail()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var query = new GetSiteScheduleQuery(siteId);
        
        var site = Site.Create(
            "Test Site",
            "123",
            "Main St",
            "12345",
            "Test City",
            "Test Country",
            "Europe/Brussels",
            DateTime.UtcNow);

        // Add a schedule to the site
        var scheduleResult = site.AddSchedule(
            "Standard Schedule",
            DateOnly.FromDateTime(DateTime.UtcNow),
            null,
            new TimeOnly(8, 0),
            new TimeOnly(22, 0),
            null,
            1,
            DateTime.UtcNow);

        scheduleResult.IsSuccess.Should().BeTrue();

        _siteRepository.GetByIdWithSchedulesAndClosuresAsync(query.SiteId, Arg.Any<CancellationToken>())
            .Returns(site);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.SiteId.Should().Be(site.Id);
        result.Value.SiteName.Should().Be("Test Site");
        result.Value.Timezone.Should().Be("Europe/Brussels");
        result.Value.Schedules.Should().HaveCount(1);
        result.Value.Schedules[0].Name.Should().Be("Standard Schedule");
        result.Value.Closures.Should().BeEmpty();
    }
}