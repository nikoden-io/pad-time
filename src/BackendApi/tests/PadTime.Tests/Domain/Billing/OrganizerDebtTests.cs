using FluentAssertions;
using PadTime.Domain.Billing;
using Xunit;

namespace PadTime.Tests.Domain.Billing;

public sealed class OrganizerDebtTests
{
    [Fact]
    public void Create_WithPositiveAmount_CreatesDebtWithOutstandingBalance()
    {
        var debt = OrganizerDebt.Create(Guid.NewGuid(), 3000, DateTime.UtcNow);

        debt.AmountCents.Should().Be(3000);
        debt.HasDebt.Should().BeTrue();
        debt.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "DebtCreatedEvent");
    }

    [Fact]
    public void IncreaseDebt_WithPositiveAmount_IncreasesBalance()
    {
        var debt = OrganizerDebt.Create(Guid.NewGuid(), 1500, DateTime.UtcNow);

        debt.IncreaseDebt(1500, DateTime.UtcNow.AddMinutes(1));

        debt.AmountCents.Should().Be(3000);
        debt.DomainEvents.Should().Contain(e => e.GetType().Name == "DebtIncreasedEvent");
    }

    [Fact]
    public void ApplyPayment_WhenPaymentClearsDebt_ReducesBalanceToZeroAndClearsDebt()
    {
        var debt = OrganizerDebt.Create(Guid.NewGuid(), 3000, DateTime.UtcNow);

        debt.ApplyPayment(4000, DateTime.UtcNow.AddMinutes(1));

        debt.AmountCents.Should().Be(0);
        debt.HasDebt.Should().BeFalse();
        debt.CanCreateMatch.Should().BeTrue();
        debt.DomainEvents.Should().Contain(e => e.GetType().Name == "DebtReducedEvent");
        debt.DomainEvents.Should().Contain(e => e.GetType().Name == "DebtClearedEvent");
    }
}
