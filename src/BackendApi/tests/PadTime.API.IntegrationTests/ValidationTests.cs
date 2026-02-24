using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using PadTime.API.Controllers;
using Xunit;

namespace PadTime.API.IntegrationTests;

/// <summary>
/// Integration tests for enhanced validation and error handling.
/// </summary>
public sealed class ValidationTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _client;

    public ValidationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateSite_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var validRequest = new CreateSiteRequest(
            Name: "Test Site",
            StreetNumber: "123",
            Street: "Main Street",
            Postcode: "12345",
            City: "Test City",
            Country: "Test Country",
            Timezone: "Europe/Brussels"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/sites", validRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        var createResponse = JsonSerializer.Deserialize<CreateSiteResponse>(content, JsonOptions);

        createResponse.Should().NotBeNull();
        createResponse!.SiteId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task NonExistentEndpoint_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/nonexistent");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateSite_WithMalformedJson_ReturnsBadRequest()
    {
        // Arrange
        var malformedJson = "{ \"name\": \"Test\", \"invalid\": }";
        var content = new StringContent(malformedJson, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/sites", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResponseHeaders_ContainCorrectContentType()
    {
        // Arrange
        var invalidRequest = new CreateSiteRequest("", "", "", "", "", "", "");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/sites", invalidRequest);

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }
}
