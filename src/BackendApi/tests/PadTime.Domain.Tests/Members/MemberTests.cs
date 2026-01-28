using FluentAssertions;
using PadTime.Domain.Members;
using Xunit;

namespace PadTime.Domain.Tests.Members;

public class MemberTests
{
    private readonly DateTime _utcNow = new(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData("G1234", 21)]
    [InlineData("S12345", 14)]
    [InlineData("L12345", 5)]
    public void GetBookingWindowDaysReturnsCorrectWindow(string matricule, int expectedDays)
    {
        // Arrange
        var siteId = matricule.StartsWith('S') ? Guid.NewGuid() : (Guid?)null;
        var result = Member.Create("sub-123", matricule, siteId, _utcNow);

        // Act & Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.GetBookingWindowDays().Should().Be(expectedDays);
    }

    [Fact]
    public void CanBookAtSiteGlobalMemberCanBookAnywhere()
    {
        // Arrange
        var member = Member.Create("sub-123", "G1234", null, _utcNow).Value;
        var anySiteId = Guid.NewGuid();

        // Act & Assert
        member.CanBookAtSite(anySiteId).Should().BeTrue();
    }

    [Fact]
    public void CanBookAtSiteSiteMemberCanOnlyBookAtAssignedSite()
    {
        // Arrange
        var assignedSiteId = Guid.NewGuid();
        var otherSiteId = Guid.NewGuid();
        var member = Member.Create("sub-123", "S12345", assignedSiteId, _utcNow).Value;

        // Act & Assert
        member.CanBookAtSite(assignedSiteId).Should().BeTrue();
        member.CanBookAtSite(otherSiteId).Should().BeFalse();
    }

    [Fact]
    public void CanBookAtSiteFreeMemberCanBookAnywhere()
    {
        // Arrange
        var member = Member.Create("sub-123", "L12345", null, _utcNow).Value;
        var anySiteId = Guid.NewGuid();

        // Act & Assert
        member.CanBookAtSite(anySiteId).Should().BeTrue();
    }

    [Fact]
    public void CanBookForDateWithinWindowReturnsTrue()
    {
        // Arrange
        var member = Member.Create("sub-123", "G1234", null, _utcNow).Value;
        var today = DateOnly.FromDateTime(_utcNow);
        var targetDate = today.AddDays(21);

        // Act & Assert
        member.CanBookForDate(targetDate, today).Should().BeTrue();
    }

    [Fact]
    public void CanBookForDateBeyondWindowReturnsFalse()
    {
        // Arrange
        var member = Member.Create("sub-123", "G1234", null, _utcNow).Value;
        var today = DateOnly.FromDateTime(_utcNow);
        var targetDate = today.AddDays(22);

        // Act & Assert
        member.CanBookForDate(targetDate, today).Should().BeFalse();
    }

    [Fact]
    public void CreateSiteMemberWithoutSiteIdReturnsFailure()
    {
        // Act
        var result = Member.Create("sub-123", "S12345", null, _utcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
