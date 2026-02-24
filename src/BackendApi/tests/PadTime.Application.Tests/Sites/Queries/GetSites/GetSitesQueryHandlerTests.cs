using FluentAssertions;
using NSubstitute;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Application.Common.Models;
using PadTime.Application.Sites.Queries.GetSites;
using PadTime.Domain.Site;
using Xunit;

namespace PadTime.Application.Tests.Sites.Queries.GetSites;

public class GetSitesQueryHandlerTests
{
    private readonly ISiteRepository _siteRepository;
    private readonly GetSitesQueryHandler _handler;
    private readonly DateTime _fixedUtcNow = new(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);

    public GetSitesQueryHandlerTests()
    {
        _siteRepository = Substitute.For<ISiteRepository>();
        _handler = new GetSitesQueryHandler(_siteRepository);
    }

    [Fact]
    public async Task Handle_NoSites_ReturnsEmptyPagedResult()
    {
        // Arrange
        _siteRepository.GetPagedAsync(
                Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<string?>(), Arg.Any<bool?>(),
                Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(PagedResult.Empty<Site>(1, 20));

        var query = new GetSitesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_MultipleSites_ReturnsAllAsDto()
    {
        // Arrange
        var site1 = CreateSite("Club 1", "Brussels");
        var site2 = CreateSite("Club 2", "Antwerp");

        var pagedResult = new PagedResult<Site>(
            [site1, site2], 1, 20, 2);

        _siteRepository.GetPagedAsync(
                Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<string?>(), Arg.Any<bool?>(),
                Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new GetSitesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items[0].Name.Should().Be("Club 1");
        result.Items[0].City.Should().Be("Brussels");
        result.Items[1].Name.Should().Be("Club 2");
        result.Items[1].City.Should().Be("Antwerp");
    }

    [Fact]
    public async Task Handle_SiteWithCourts_IncludesCourtsDtoOrderedByLabel()
    {
        // Arrange
        var site = CreateSite("Club", "Brussels");
        site.AddCourt("Court C", _fixedUtcNow);
        site.AddCourt("Court A", _fixedUtcNow);
        site.AddCourt("Court B", _fixedUtcNow);

        var pagedResult = new PagedResult<Site>(
            [site], 1, 20, 1);

        _siteRepository.GetPagedAsync(
                Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<string?>(), Arg.Any<bool?>(),
                Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new GetSitesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Courts.Should().HaveCount(3);
        result.Items[0].Courts[0].Label.Should().Be("Court A");
        result.Items[0].Courts[1].Label.Should().Be("Court B");
        result.Items[0].Courts[2].Label.Should().Be("Court C");
    }

    [Fact]
    public async Task Handle_ValidQuery_MapsAllPropertiesCorrectly()
    {
        // Arrange
        var site = CreateSite("Test Club", "Brussels");

        var pagedResult = new PagedResult<Site>(
            [site], 1, 20, 1);

        _siteRepository.GetPagedAsync(
                Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<string?>(), Arg.Any<bool?>(),
                Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new GetSitesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var dto = result.Items[0];
        dto.SiteId.Should().Be(site.Id);
        dto.Name.Should().Be("Test Club");
        dto.StreetNumber.Should().Be("123");
        dto.Street.Should().Be("Test Street");
        dto.Postcode.Should().Be("1000");
        dto.City.Should().Be("Brussels");
        dto.Country.Should().Be("Belgium");
        dto.Timezone.Should().Be("Europe/Brussels");
    }

    [Fact]
    public async Task Handle_PassesQueryParametersToRepository()
    {
        // Arrange
        _siteRepository.GetPagedAsync(
                Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<string?>(), Arg.Any<bool?>(),
                Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(PagedResult.Empty<Site>(2, 10));

        var query = new GetSitesQuery(
            Page: 2,
            PageSize: 10,
            SearchTerm: "test",
            IsActive: true,
            City: "Brussels",
            Country: "Belgium");

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _siteRepository.Received(1).GetPagedAsync(
            2, 10, "test", true, "Brussels", "Belgium",
            Arg.Any<CancellationToken>());
    }

    private Site CreateSite(string name, string city)
    {
        return Site.Create(
            name,
            "123",
            "Test Street",
            "1000",
            city,
            "Belgium",
            "Europe/Brussels",
            _fixedUtcNow);
    }
}
