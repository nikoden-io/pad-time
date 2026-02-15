using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PadTime.API.Controllers;
using PadTime.Domain.Site;
using Xunit;
using Xunit.Abstractions;

namespace PadTime.API.IntegrationTests;

public class SitesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly HttpClient _client;

    public SitesControllerTests(CustomWebApplicationFactory factory, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSites_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/sites");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task GetSiteById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidSiteId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/sites/{invalidSiteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateSite_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidSiteId = Guid.NewGuid();
        var updateRequest = new UpdateSiteRequest(
            "Updated Site",
            "123",
            "Updated Street",
            "12345",
            "Updated City",
            "Updated Country",
            "UTC"
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/sites/{invalidSiteId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteSite_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidSiteId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/sites/{invalidSiteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeactivateSite_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidSiteId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync($"/api/v1/sites/{invalidSiteId}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateSite_WithValidData_ReturnsCreated()
    {
        // Arrange
        var createRequest = new CreateSiteRequest(
            "Test Site",
            "123",
            "Test Street",
            "12345",
            "Test City",
            "Test Country",
            "UTC"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/sites", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCourtById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidSiteId = Guid.NewGuid();
        var invalidCourtId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/sites/{invalidSiteId}/courts/{invalidCourtId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCourt_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidSiteId = Guid.NewGuid();
        var invalidCourtId = Guid.NewGuid();
        var updateRequest = new UpdateCourtRequest("Updated Court");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/sites/{invalidSiteId}/courts/{invalidCourtId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCourt_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidSiteId = Guid.NewGuid();
        var invalidCourtId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/sites/{invalidSiteId}/courts/{invalidCourtId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSiteSchedule_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidSiteId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/sites/{invalidSiteId}/schedules");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateSiteSchedule_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidSiteId = Guid.NewGuid();
        var scheduleId = Guid.NewGuid();
        var updateRequest = new UpdateSiteScheduleRequest(
            "Test Schedule",
            DateOnly.FromDateTime(DateTime.Today),
            null,
            new TimeOnly(9, 0),
            new TimeOnly(22, 0),
            [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday],
            1
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/sites/{invalidSiteId}/schedules/{scheduleId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddSiteClosure_WithInvalidId_ReturnsNotFound()
    {
        var invalidSiteId = Guid.NewGuid();
        var addClosureRequest = new AddSiteClosureRequest(
            ClosureType.FullDay,
            ClosureReason.PublicHoliday,
            "Christmas Day",
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(30)),
            null,
            null,
            null,
            null
        );

        var response = await _client.PostAsJsonAsync($"/api/v1/sites/{invalidSiteId}/closures", addClosureRequest);

        var content = await response.Content.ReadAsStringAsync();
        _testOutputHelper.WriteLine($"Status: {response.StatusCode}, Body: {content}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveSiteClosure_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidSiteId = Guid.NewGuid();
        var invalidClosureId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/sites/{invalidSiteId}/closures/{invalidClosureId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
