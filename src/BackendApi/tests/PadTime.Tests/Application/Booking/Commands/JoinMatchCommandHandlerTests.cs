using FluentAssertions;
using NSubstitute;
using PadTime.Application.Booking.Commands.JoinMatch;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Billing;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;
using PadTime.Domain.Members;
using Xunit;

namespace PadTime.Tests.Application.Booking.Commands;

public sealed class JoinMatchCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenPaymentAlreadyExists_ReturnsExistingPaymentWithoutMutatingMatch()
    {
        var existingPayment = Payment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            1500,
            PaymentPurpose.MatchParticipation,
            "idem-1",
            DateTime.UtcNow).Value;

        var matchRepository = Substitute.For<IMatchRepository>();
        var memberRepository = Substitute.For<IMemberRepository>();
        var paymentRepository = Substitute.For<IPaymentRepository>();
        var currentUser = CreateCurrentUser();
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        paymentRepository.GetByIdempotencyKeyAsync("idem-1", Arg.Any<CancellationToken>())
            .Returns(existingPayment);

        var handler = new JoinMatchCommandHandler(
            matchRepository,
            memberRepository,
            paymentRepository,
            currentUser,
            dateTimeProvider,
            unitOfWork);

        var result = await handler.Handle(new JoinMatchCommand(Guid.NewGuid(), "idem-1"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PaymentId.Should().Be(existingPayment.Id);
        result.Value.Status.Should().Be("pending");
        await matchRepository.DidNotReceive().GetByIdWithParticipantsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenJoiningPublicMatch_CreatesPaymentAndSavesChanges()
    {
        var now = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
        var member = Member.Create("subject-1", "G1234", null, now).Value;
        var match = Match.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            now.AddDays(1),
            now.AddDays(1).AddMinutes(90),
            PadMatchType.Public,
            now).Value;

        var matchRepository = Substitute.For<IMatchRepository>();
        var memberRepository = Substitute.For<IMemberRepository>();
        var paymentRepository = Substitute.For<IPaymentRepository>();
        var currentUser = CreateCurrentUser();
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        dateTimeProvider.UtcNow.Returns(now);
        paymentRepository.GetByIdempotencyKeyAsync("idem-1", Arg.Any<CancellationToken>())
            .Returns((Payment?)null);
        memberRepository.GetBySubjectAsync(currentUser.Subject, Arg.Any<CancellationToken>())
            .Returns(member);
        matchRepository.GetByIdWithParticipantsAsync(match.Id, Arg.Any<CancellationToken>())
            .Returns(match);

        var handler = new JoinMatchCommandHandler(
            matchRepository,
            memberRepository,
            paymentRepository,
            currentUser,
            dateTimeProvider,
            unitOfWork);

        var result = await handler.Handle(new JoinMatchCommand(match.Id, "idem-1"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("paid");
        match.Participants.Should().Contain(p => p.MemberId == member.Id && p.PaymentStatus == PaymentStatus.Paid);
        await paymentRepository.Received(1).AddAsync(
            Arg.Is<Payment>(p =>
                p.MatchId == match.Id &&
                p.MemberId == member.Id &&
                p.State == PaymentState.Paid &&
                p.IdempotencyKey == "idem-1"),
            Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCurrentMemberIsInactive_ReturnsInactive()
    {
        var now = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
        var member = Member.Create("subject-1", "G1234", null, now).Value;
        member.Deactivate(now);

        var matchRepository = Substitute.For<IMatchRepository>();
        var memberRepository = Substitute.For<IMemberRepository>();
        var paymentRepository = Substitute.For<IPaymentRepository>();
        var currentUser = CreateCurrentUser();
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        dateTimeProvider.UtcNow.Returns(now);
        paymentRepository.GetByIdempotencyKeyAsync("idem-1", Arg.Any<CancellationToken>())
            .Returns((Payment?)null);
        memberRepository.GetBySubjectAsync(currentUser.Subject, Arg.Any<CancellationToken>())
            .Returns(member);

        var handler = new JoinMatchCommandHandler(
            matchRepository,
            memberRepository,
            paymentRepository,
            currentUser,
            dateTimeProvider,
            unitOfWork);

        var result = await handler.Handle(new JoinMatchCommand(Guid.NewGuid(), "idem-1"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Should().Be(DomainErrors.Member.Inactive);
        await paymentRepository.DidNotReceive().AddAsync(Arg.Any<Payment>(), Arg.Any<CancellationToken>());
    }

    private static ICurrentUser CreateCurrentUser()
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.Subject.Returns("subject-1");
        currentUser.Matricule.Returns("G1234");
        currentUser.IsAuthenticated.Returns(true);
        return currentUser;
    }
}
