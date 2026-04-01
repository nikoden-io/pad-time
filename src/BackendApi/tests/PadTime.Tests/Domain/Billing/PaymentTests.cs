using FluentAssertions;
using PadTime.Domain.Billing;
using PadTime.Domain.Common;
using Xunit;

namespace PadTime.Tests.Domain.Billing;

public sealed class PaymentTests
{
    [Fact]
    public void Create_WithValidInput_CreatesPendingPayment()
    {
        var result = Payment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            1500,
            PaymentPurpose.MatchParticipation,
            "idem-1",
            DateTime.UtcNow);

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(PaymentState.Pending);
        result.Value.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "PaymentCreatedEvent");
    }

    [Fact]
    public void MarkAsPaid_WhenPending_TransitionsToPaid()
    {
        var payment = CreatePayment();

        var result = payment.MarkAsPaid(DateTime.UtcNow);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentState.Paid);
        payment.ProcessedAtUtc.Should().NotBeNull();
        payment.DomainEvents.Should().Contain(e => e.GetType().Name == "PaymentSucceededEvent");
    }

    [Fact]
    public void MarkAsFailed_WhenPending_TransitionsToFailed()
    {
        var payment = CreatePayment();

        var result = payment.MarkAsFailed(DateTime.UtcNow);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentState.Failed);
        payment.ProcessedAtUtc.Should().NotBeNull();
        payment.DomainEvents.Should().Contain(e => e.GetType().Name == "PaymentFailedEvent");
    }

    [Fact]
    public void MarkAsPaid_WhenAlreadyProcessed_ReturnsPaymentAlreadyProcessed()
    {
        var payment = CreatePayment();
        payment.MarkAsPaid(DateTime.UtcNow).IsSuccess.Should().BeTrue();

        var result = payment.MarkAsPaid(DateTime.UtcNow.AddMinutes(1));

        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Should().Be(DomainErrors.Billing.PaymentAlreadyProcessed);
    }

    private static Payment CreatePayment()
    {
        return Payment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            1500,
            PaymentPurpose.MatchParticipation,
            "idem-1",
            DateTime.UtcNow).Value;
    }
}
