using FluentAssertions;
using PadTime.Domain.Booking;
using Xunit;

namespace PadTime.Tests.Domain.Common;

// Exercises the ValueObject base class through a concrete value object (TimeSlot).
public sealed class ValueObjectEqualityTests
{
    private static TimeSlot Slot(int day = 1, int hour = 8)
        => new(new DateOnly(2026, 4, day), new TimeOnly(hour, 0), new TimeOnly(hour + 1, 30));

    [Fact]
    public void Equals_WhenComponentsMatch_AreEqual()
    {
        Slot().Should().Be(Slot());
        Slot().GetHashCode().Should().Be(Slot().GetHashCode());
    }

    [Fact]
    public void Equals_WhenComponentsDiffer_AreNotEqual()
    {
        Slot(day: 1).Should().NotBe(Slot(day: 2));
        Slot(hour: 8).Should().NotBe(Slot(hour: 10));
    }

    [Fact]
    public void Equals_WhenComparedToNullOrOtherType_ReturnsFalse()
    {
        Slot().Equals(null).Should().BeFalse();
        Slot().Equals("not a slot").Should().BeFalse();
    }
}
