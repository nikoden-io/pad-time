using FluentAssertions;
using PadTime.Domain.Common;
using PadTime.Domain.Site;
using Xunit;

namespace PadTime.Tests.Domain.Site;

public sealed class SiteScheduleTests
{
    private static readonly DateTime Clock = new(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly From = new(2026, 4, 1);

    private static Result<SiteSchedule> Create(
        TimeOnly? open = null, TimeOnly? close = null,
        DateOnly? validUntil = null, DayOfWeek[]? days = null, string name = "Default")
        => SiteSchedule.Create(Guid.NewGuid(), name, From, validUntil,
            open ?? new TimeOnly(8, 0), close ?? new TimeOnly(22, 0), days, 0, Clock);

    [Fact]
    public void Create_WhenValid_Succeeds()
    {
        var result = Create();
        result.IsSuccess.Should().BeTrue();
        result.Value.OpeningTime.Should().Be(new TimeOnly(8, 0));
    }

    [Fact]
    public void Create_WhenClosingNotAfterOpening_ReturnsInvalidSchedule()
    {
        Create(open: new TimeOnly(22, 0), close: new TimeOnly(8, 0))
            .PadTimeError.Should().Be(DomainErrors.Site.InvalidSchedule);
    }

    [Fact]
    public void Create_WhenValidUntilBeforeValidFrom_ReturnsInvalidSchedule()
    {
        Create(validUntil: From.AddDays(-1))
            .PadTimeError.Should().Be(DomainErrors.Site.InvalidSchedule);
    }

    [Fact]
    public void Create_WhenNameBlank_ReturnsInvalidSchedule()
    {
        Create(name: "   ").PadTimeError.Should().Be(DomainErrors.Site.InvalidSchedule);
    }

    [Fact]
    public void Create_WhenApplicableDaysEmpty_ReturnsInvalidSchedule()
    {
        Create(days: System.Array.Empty<DayOfWeek>())
            .PadTimeError.Should().Be(DomainErrors.Site.InvalidSchedule);
    }

    [Fact]
    public void IsApplicableOn_RespectsValidityAndDays()
    {
        var schedule = Create(days: new[] { From.DayOfWeek }).Value;
        schedule.IsApplicableOn(From).Should().BeTrue();
        schedule.IsApplicableOn(From.AddDays(-1)).Should().BeFalse(); // before validFrom
        var otherDay = Create(days: new[] { From.AddDays(1).DayOfWeek }).Value;
        otherDay.IsApplicableOn(From).Should().BeFalse(); // wrong day of week
    }

    [Fact]
    public void GenerateSlots_OnApplicableDate_ProducesNinetyMinuteSlots()
    {
        var schedule = Create().Value;
        var slots = schedule.GenerateSlots(From).ToList();

        slots.Should().NotBeEmpty();
        slots[0].StartTime.Should().Be(new TimeOnly(8, 0));
        slots.Should().OnlyContain(s => (s.EndTime - s.StartTime) == System.TimeSpan.FromMinutes(90));
    }

    [Fact]
    public void GenerateSlots_OnInapplicableDate_IsEmpty()
    {
        var schedule = Create().Value;
        schedule.GenerateSlots(From.AddDays(-5)).Should().BeEmpty();
    }
}
