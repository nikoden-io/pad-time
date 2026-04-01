using FluentAssertions;
using PadTime.Domain.Common;
using PadTime.Domain.Members;
using Xunit;

namespace PadTime.Tests.Domain.Members;

public sealed class MemberTests
{
    [Fact]
    public void Create_WithValidGlobalMatricule_CreatesActiveMemberWithoutSiteRestriction()
    {
        var result = Member.Create("subject-1", "g1234", Guid.NewGuid(), DateTime.UtcNow);

        result.IsSuccess.Should().BeTrue();
        result.Value.Category.Should().Be(MemberCategory.Global);
        result.Value.SiteId.Should().BeNull();
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithBlankSubject_ReturnsInvalidMatricule()
    {
        var result = Member.Create(string.Empty, "G1234", null, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Should().Be(DomainErrors.Member.InvalidMatricule);
    }

    [Fact]
    public void Create_WithSiteMatriculeAndNoSite_ReturnsSiteScopeViolation()
    {
        var result = Member.Create("subject-1", "S12345", null, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Should().Be(DomainErrors.Booking.SiteScopeViolation);
    }

    [Theory]
    [InlineData("G1234", MemberCategory.Global, 21)]
    [InlineData("S12345", MemberCategory.Site, 14)]
    [InlineData("L12345", MemberCategory.Free, 5)]
    public void Create_WhenMatriculePrefixVaries_DerivesCategoryAndBookingWindow(
        string matricule,
        MemberCategory expectedCategory,
        int expectedDays)
    {
        var result = Member.Create("subject-1", matricule, Guid.NewGuid(), DateTime.UtcNow);

        result.IsSuccess.Should().BeTrue();
        result.Value.Category.Should().Be(expectedCategory);
        result.Value.GetBookingWindowDays().Should().Be(expectedDays);
    }

    [Fact]
    public void CanBookForDate_WhenWithinBookingWindow_ReturnsTrue()
    {
        var member = Member.Create("subject-1", "S12345", Guid.NewGuid(), DateTime.UtcNow).Value;
        var today = new DateOnly(2026, 4, 1);

        var result = member.CanBookForDate(today.AddDays(14), today);

        result.Should().BeTrue();
    }

    [Fact]
    public void CanBookForDate_WhenOutsideBookingWindow_ReturnsFalse()
    {
        var member = Member.Create("subject-1", "L12345", null, DateTime.UtcNow).Value;
        var today = new DateOnly(2026, 4, 1);

        var result = member.CanBookForDate(today.AddDays(6), today);

        result.Should().BeFalse();
    }
}
