using FluentAssertions;
using PadTime.Domain.Site;
using Xunit;

namespace PadTime.Domain.Tests.Booking;

using Site = PadTime.Domain.Site.Site;

public class SiteTests
{
    private readonly DateTime _utcNow = new(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_ValidParameters_ReturnsActiveSite()
    {
        // Act
        var site = Site.Create(
            "Test Club",
            "123",
            "Test Street",
            "1000",
            "Brussels",
            "Belgium",
            "Europe/Brussels",
            _utcNow);

        // Assert
        site.Should().NotBeNull();
        site.Name.Should().Be("Test Club");
        site.City.Should().Be("Brussels");
        site.IsActive.Should().BeTrue();
        site.CreatedAtUtc.Should().Be(_utcNow);
        site.Courts.Should().BeEmpty();
    }

    [Fact]
    public void AddCourt_ValidLabel_AddsCourt()
    {
        // Arrange
        var site = CreateSite();

        // Act
        var court = site.AddCourt("Court 1", _utcNow);

        // Assert
        court.Should().NotBeNull();
        court.Label.Should().Be("Court 1");
        site.Courts.Should().HaveCount(1);
        site.Courts[0].Should().Be(court);
    }

    [Fact]
    public void AddCourt_MultipleCourts_AddsAllCourts()
    {
        // Arrange
        var site = CreateSite();

        // Act
        site.AddCourt("Court 1", _utcNow);
        site.AddCourt("Court 2", _utcNow);
        site.AddCourt("Court 3", _utcNow);

        // Assert
        site.Courts.Should().HaveCount(3);
        site.Courts[0].Label.Should().Be("Court 1");
        site.Courts[1].Label.Should().Be("Court 2");
        site.Courts[2].Label.Should().Be("Court 3");
    }

    [Fact]
    public void Deactivate_ActiveSite_SetsIsActiveToFalse()
    {
        // Arrange
        var site = CreateSite();

        // Act
        site.Deactivate(_utcNow);

        // Assert
        site.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_InactiveSite_SetsIsActiveToTrue()
    {
        // Arrange
        var site = CreateSite();
        site.Deactivate(_utcNow);

        // Act
        site.Activate(_utcNow);

        // Assert
        site.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsClosedOn_DateInClosuresList_ReturnsTrue()
    {
        // Arrange
        var site = CreateSite();
        var closureDate = new DateOnly(2025, 12, 25);
        var result = site.AddFullDayClosure(
            closureDate,
            ClosureReason.PublicHoliday,
            "Christmas",
            null,
            _utcNow);

        result.IsSuccess.Should().BeTrue();

        // Act
        var isClosed = site.IsClosedOn(closureDate);

        // Assert
        isClosed.Should().BeTrue();
    }

    [Fact]
    public void IsClosedOn_DateNotInClosuresList_ReturnsFalse()
    {
        // Arrange
        var site = CreateSite();
        var closureDate = new DateOnly(2025, 12, 25);
        site.AddFullDayClosure(
            closureDate,
            ClosureReason.PublicHoliday,
            "Christmas",
            null,
            _utcNow);

        // Act
        var result = site.IsClosedOn(new DateOnly(2025, 12, 26));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AddSchedule_ValidSchedule_AddsSchedule()
    {
        // Arrange
        var site = CreateSite();

        // Act
        var result = site.AddSchedule(
            "Standard Schedule",
            new DateOnly(2025, 1, 1),
            new DateOnly(2025, 12, 31),
            new TimeOnly(8, 0),
            new TimeOnly(22, 0),
            null,
            1,
            _utcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        site.Schedules.Should().HaveCount(1);
        site.Schedules[0].Name.Should().Be("Standard Schedule");
        site.Schedules[0].OpeningTime.Should().Be(new TimeOnly(8, 0));
        site.Schedules[0].ClosingTime.Should().Be(new TimeOnly(22, 0));
    }

    [Fact]
    public void AddSchedule_ConflictingSchedule_ReturnsFailure()
    {
        // Arrange
        var site = CreateSite();
        site.AddSchedule(
            "Standard Schedule",
            new DateOnly(2025, 1, 1),
            new DateOnly(2025, 12, 31),
            new TimeOnly(8, 0),
            new TimeOnly(22, 0),
            null,
            1,
            _utcNow);

        // Act - add another schedule with same priority and overlapping dates
        var result = site.AddSchedule(
            "Conflicting Schedule",
            new DateOnly(2025, 6, 1),
            new DateOnly(2025, 12, 31),
            new TimeOnly(9, 0),
            new TimeOnly(23, 0),
            null,
            1,
            _utcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void GetScheduleForDate_ScheduleExists_ReturnsSchedule()
    {
        // Arrange
        var site = CreateSite();
        site.AddSchedule(
            "Standard Schedule",
            new DateOnly(2025, 1, 1),
            new DateOnly(2025, 12, 31),
            new TimeOnly(8, 0),
            new TimeOnly(22, 0),
            null,
            1,
            _utcNow);

        // Act
        var result = site.GetScheduleForDate(new DateOnly(2025, 6, 15));

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Standard Schedule");
    }

    [Fact]
    public void GetScheduleForDate_ScheduleDoesNotExist_ReturnsNull()
    {
        // Arrange
        var site = CreateSite();

        // Act
        var result = site.GetScheduleForDate(new DateOnly(2025, 6, 15));

        // Assert
        result.Should().BeNull();
    }

    private Site CreateSite()
    {
        return Site.Create(
            "Default Club",
            "123",
            "Test Street",
            "1000",
            "Brussels",
            "Belgium",
            "Europe/Brussels",
            _utcNow);
    }
}
