using FluentAssertions;
using NSubstitute;
using PadTime.Application.Billing.Commands.PayMatchParticipation;
using PadTime.Application.Billing.Queries.GetPayment;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Billing;
using PadTime.Domain.Booking;
using PadTime.Domain.Members;
using Xunit;

namespace PadTime.Tests.Application.Billing;

public sealed class BillingHandlersTests
{
    [Fact]
    public async Task PayMatchParticipation_WhenParticipantIsUnpaid_CreatesPaidPayment()
    {
        var now = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
        var member = Member.Create("subject", "G1234", null, now).Value;
        var match = Match.Create(Guid.NewGuid(), Guid.NewGuid(), member.Id, now.AddDays(1), now.AddDays(1).AddMinutes(90), PadMatchType.Private, now).Value;
        var secondMember = Member.Create("subject-2", "G2345", null, now).Value;
        match.AddParticipant(secondMember.Id, now).IsSuccess.Should().BeTrue();

        var matchRepository = Substitute.For<IMatchRepository>();
        var memberRepository = Substitute.For<IMemberRepository>();
        var paymentRepository = Substitute.For<IPaymentRepository>();
        var currentUser = Substitute.For<ICurrentUser>();
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        currentUser.Subject.Returns("subject");
        dateTimeProvider.UtcNow.Returns(now);
        paymentRepository.GetByIdempotencyKeyAsync("idem", Arg.Any<CancellationToken>()).Returns((Payment?)null);
        memberRepository.GetBySubjectAsync("subject", Arg.Any<CancellationToken>()).Returns(member);
        matchRepository.GetByIdWithParticipantsAsync(match.Id, Arg.Any<CancellationToken>()).Returns(match);

        var handler = new PayMatchParticipationCommandHandler(matchRepository, memberRepository, paymentRepository, currentUser, dateTimeProvider, unitOfWork);

        var result = await handler.Handle(new PayMatchParticipationCommand(match.Id, "idem"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("paid");
        await paymentRepository.Received(1).AddAsync(Arg.Is<Payment>(p => p.MemberId == member.Id && p.State == PaymentState.Paid), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPayment_WhenNonOwnerRequestsPayment_ReturnsNotFound()
    {
        var payment = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1500, PaymentPurpose.MatchParticipation, "idem", DateTime.UtcNow).Value;
        var paymentRepository = Substitute.For<IPaymentRepository>();
        var memberRepository = Substitute.For<IMemberRepository>();
        var currentUser = Substitute.For<ICurrentUser>();

        paymentRepository.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);
        currentUser.IsAdmin.Returns(false);
        currentUser.Subject.Returns("subject");
        memberRepository.GetBySubjectAsync("subject", Arg.Any<CancellationToken>()).Returns(Member.Create("subject", "G1234", null, DateTime.UtcNow).Value);

        var handler = new GetPaymentQueryHandler(paymentRepository, memberRepository, currentUser);

        var result = await handler.Handle(new GetPaymentQuery(payment.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Should().Be(DomainErrors.Billing.PaymentNotFound);
    }
}
