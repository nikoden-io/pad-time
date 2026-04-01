using FluentAssertions;
using PadTime.Domain.Booking;
using Xunit;

namespace PadTime.Tests.Domain.Booking;

public sealed class TimeSlotTests
{
    [Fact]
    public void Constructor_WithValidRange_CreatesSlotWithExpectedDuration()
    {
        var slot = new TimeSlot(
            new DateOnly(2026, 4, 10),
            new TimeOnly(9, 0),
            new TimeOnly(10, 30));

        slot.Duration.Should().Be(TimeSpan.FromMinutes(90));
        slot.ToString().Should().Be("2026-04-10 09:00-10:30");
    }

    [Fact]
    public void FromDateTimes_WithInvalidRange_ThrowsArgumentException()
    {
        var start = new DateTime(2026, 4, 10, 10, 0, 0);
        var end = start;

        var action = () => TimeSlot.FromDateTimes(start, end);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ToUtcStartAndToUtcEnd_WithUtcTimeZone_ReturnExpectedDateTimes()
    {
        var slot = new TimeSlot(
            new DateOnly(2026, 4, 10),
            new TimeOnly(9, 0),
            new TimeOnly(10, 30));

        slot.ToUtcStart(TimeZoneInfo.Utc).Should().Be(new DateTime(2026, 4, 10, 9, 0, 0, DateTimeKind.Utc));
        slot.ToUtcEnd(TimeZoneInfo.Utc).Should().Be(new DateTime(2026, 4, 10, 10, 30, 0, DateTimeKind.Utc));
    }
}
