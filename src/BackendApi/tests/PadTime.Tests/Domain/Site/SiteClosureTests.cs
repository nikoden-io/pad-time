using FluentAssertions;
using PadTime.Domain.Common;
using PadTime.Domain.Site;
using Xunit;

namespace PadTime.Tests.Domain.Site;

public sealed class SiteClosureTests
{
    private static readonly DateTime Clock = new(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Day = new(2026, 4, 10);
    private static readonly Guid SiteId = Guid.NewGuid();

    [Fact]
    public void CreateFullDayClosure_IsValidAndAffectsThatDayOnly()
    {
        var closure = SiteClosure.CreateFullDayClosure(
            SiteId, Day, ClosureReason.PublicHoliday, "Holiday", null, Clock).Value;

        closure.AffectsDate(Day).Should().BeTrue();
        closure.AffectsDate(Day.AddDays(1)).Should().BeFalse();
        closure.IsFullyClosed(Day).Should().BeTrue();
    }

    [Fact]
    public void CreatePeriodClosure_WhenEndBeforeStart_ReturnsInvalidClosure()
    {
        SiteClosure.CreatePeriodClosure(
            SiteId, Day, Day.AddDays(-2), ClosureReason.Vacation, null, null, Clock)
            .PadTimeError.Should().Be(DomainErrors.Site.InvalidClosure);
    }

    [Fact]
    public void CreatePeriodClosure_WhenValid_AffectsEveryDayInRange()
    {
        var closure = SiteClosure.CreatePeriodClosure(
            SiteId, Day, Day.AddDays(3), ClosureReason.Vacation, null, null, Clock).Value;

        closure.AffectsDate(Day).Should().BeTrue();
        closure.AffectsDate(Day.AddDays(2)).Should().BeTrue();
        closure.AffectsDate(Day.AddDays(5)).Should().BeFalse();
    }

    [Fact]
    public void CreateReducedHours_WhenClosingNotAfterOpening_ReturnsInvalidClosure()
    {
        SiteClosure.CreateReducedHours(
            SiteId, Day, new TimeOnly(20, 0), new TimeOnly(10, 0),
            ClosureReason.Vacation, null, null, Clock)
            .PadTimeError.Should().Be(DomainErrors.Site.InvalidClosure);
    }

    [Fact]
    public void CreateReducedHours_WhenValid_ExposesModifiedHoursAndIsNotFullyClosed()
    {
        var closure = SiteClosure.CreateReducedHours(
            SiteId, Day, new TimeOnly(10, 0), new TimeOnly(16, 0),
            ClosureReason.Vacation, "Maintenance", null, Clock).Value;

        closure.GetModifiedHours(Day).Should().Be((new TimeOnly(10, 0), new TimeOnly(16, 0)));
        closure.IsFullyClosed(Day).Should().BeFalse();
    }

    [Fact]
    public void AffectsCourt_WhenNoCourtsSpecified_AffectsAllCourts()
    {
        var closure = SiteClosure.CreateFullDayClosure(
            SiteId, Day, ClosureReason.PublicHoliday, null, null, Clock).Value;

        closure.AffectsCourt(Guid.NewGuid()).Should().BeTrue();
    }

    [Fact]
    public void AffectsCourt_WhenSpecificCourts_AffectsOnlyThose()
    {
        var court = Guid.NewGuid();
        var closure = SiteClosure.CreateFullDayClosure(
            SiteId, Day, ClosureReason.PublicHoliday, null, new[] { court }, Clock).Value;

        closure.AffectsCourt(court).Should().BeTrue();
        closure.AffectsCourt(Guid.NewGuid()).Should().BeFalse();
    }
}
