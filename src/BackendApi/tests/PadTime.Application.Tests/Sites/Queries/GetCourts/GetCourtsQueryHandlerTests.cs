using FluentAssertions;
using NSubstitute;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Application.Sites.Queries.GetCourts;
using PadTime.Domain.Common;
using PadTime.Domain.Site;
using Xunit;

namespace PadTime.Application.Tests.Sites.Queries.GetCourts;

public class GetCourtsQueryHandlerTests
{
    private readonly ISiteRepository _siteRepository;
    private readonly GetCourtsQueryHandler _handler;
    private readonly DateTime _fixedUtcNow = new(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);
    private readonly Guid _siteId = Guid.NewGuid();

    public GetCourtsQueryHandlerTests()
    {
        _siteRepository = Substitute.For<ISiteRepository>();
        _handler = new GetCourtsQueryHandler(_siteRepository);
    }

    [Fact]
    public async Task Handle_SiteNotFound_ReturnsFailure()
    {
        // Arrange
        _siteRepository.GetByIdAsync(_siteId, Arg.Any<CancellationToken>())
            .Returns((Site?)null);

        var query = new GetCourtsQuery(_siteId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Code.Should().Be(DomainErrors.Site.NotFound.Code);
    }

    [Fact]
    public async Task Handle_NoCourts_ReturnsEmptyList()
    {
        // Arrange
        var site = CreateSite();
        _siteRepository.GetByIdAsync(site.Id, Arg.Any<CancellationToken>())
            .Returns(site);

        var query = new GetCourtsQuery(site.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MultipleCourts_ReturnsAllAsDtoOrderedByLabel()
    {
        // Arrange
        var site = CreateSite();
        site.AddCourt("Court B", _fixedUtcNow);
        site.AddCourt("Court A", _fixedUtcNow);

        _siteRepository.GetByIdAsync(site.Id, Arg.Any<CancellationToken>())
            .Returns(site);

        var query = new GetCourtsQuery(site.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Label.Should().Be("Court A");
        result.Value[1].Label.Should().Be("Court B");
    }

    [Fact]
    public async Task Handle_ValidQuery_MapsAllPropertiesCorrectly()
    {
        // Arrange
        var site = CreateSite();
        var court = site.AddCourt("Test Court", _fixedUtcNow);

        _siteRepository.GetByIdAsync(site.Id, Arg.Any<CancellationToken>())
            .Returns(site);

        var query = new GetCourtsQuery(site.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value[0];
        dto.CourtId.Should().Be(court.Id);
        dto.Label.Should().Be("Test Court");
        dto.IsActive.Should().BeTrue();
    }

    private Site CreateSite()
    {
        return Site.Create(
            "Test Site",
            "123",
            "Test Street",
            "1000",
            "Brussels",
            "Belgium",
            "Europe/Brussels",
            _fixedUtcNow);
    }
}
