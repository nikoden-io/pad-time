using FluentAssertions;
using NSubstitute;
using PadTime.Application.Booking.Commands.CancelMatch;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;
using PadTime.Domain.Members;
using Xunit;

namespace PadTime.Tests.Application.Booking.Commands;

public sealed class CancelMatchCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenOrganizerCancelsUnlockedMatch_CancelsAndSaves()
    {
        var now = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
        var organizer = Member.Create("subject-1", "G1234", null, now).Value;
        var match = Match.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            organizer.Id,
            now.AddDays(1),
            now.AddDays(1).AddMinutes(90),
            PadMatchType.Private,
            now).Value;

        var matchRepository = Substitute.For<IMatchRepository>();
        var memberRepository = Substitute.For<IMemberRepository>();
        var currentUser = CreateCurrentUser(isAdmin: false, isSiteAdmin: false, isGlobalAdmin: false, siteId: null);
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        dateTimeProvider.UtcNow.Returns(now);
        matchRepository.GetByIdWithParticipantsAsync(match.Id, Arg.Any<CancellationToken>())
            .Returns(match);
        memberRepository.GetBySubjectAsync(currentUser.Subject, Arg.Any<CancellationToken>())
            .Returns(organizer);

        var handler = new CancelMatchCommandHandler(
            matchRepository,
            memberRepository,
            currentUser,
            dateTimeProvider,
            unitOfWork);

        var result = await handler.Handle(new CancelMatchCommand(match.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        match.Status.Should().Be(MatchStatus.Cancelled);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNonOrganizerCancelsMatch_ReturnsNotOrganizer()
    {
        var now = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
        var organizer = Member.Create("organizer-subject", "G1234", null, now).Value;
        var outsider = Member.Create("subject-1", "G2345", null, now).Value;
        var match = Match.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            organizer.Id,
            now.AddDays(1),
            now.AddDays(1).AddMinutes(90),
            PadMatchType.Private,
            now).Value;

        var matchRepository = Substitute.For<IMatchRepository>();
        var memberRepository = Substitute.For<IMemberRepository>();
        var currentUser = CreateCurrentUser(isAdmin: false, isSiteAdmin: false, isGlobalAdmin: false, siteId: null);
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        dateTimeProvider.UtcNow.Returns(now);
        matchRepository.GetByIdWithParticipantsAsync(match.Id, Arg.Any<CancellationToken>())
            .Returns(match);
        memberRepository.GetBySubjectAsync(currentUser.Subject, Arg.Any<CancellationToken>())
            .Returns(outsider);

        var handler = new CancelMatchCommandHandler(
            matchRepository,
            memberRepository,
            currentUser,
            dateTimeProvider,
            unitOfWork);

        var result = await handler.Handle(new CancelMatchCommand(match.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Should().Be(DomainErrors.Booking.NotOrganizer);
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSiteAdminCancelsOtherSiteMatch_ReturnsSiteScopeViolation()
    {
        var now = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
        var match = Match.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            now.AddDays(1),
            now.AddDays(1).AddMinutes(90),
            PadMatchType.Public,
            now).Value;
        var siteAdminSiteId = Guid.NewGuid();

        var matchRepository = Substitute.For<IMatchRepository>();
        var memberRepository = Substitute.For<IMemberRepository>();
        var currentUser = CreateCurrentUser(isAdmin: true, isSiteAdmin: true, isGlobalAdmin: false, siteId: siteAdminSiteId);
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        dateTimeProvider.UtcNow.Returns(now);
        matchRepository.GetByIdWithParticipantsAsync(match.Id, Arg.Any<CancellationToken>())
            .Returns(match);

        var handler = new CancelMatchCommandHandler(
            matchRepository,
            memberRepository,
            currentUser,
            dateTimeProvider,
            unitOfWork);

        var result = await handler.Handle(new CancelMatchCommand(match.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Should().Be(DomainErrors.Booking.SiteScopeViolation);
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static ICurrentUser CreateCurrentUser(bool isAdmin, bool isSiteAdmin, bool isGlobalAdmin, Guid? siteId)
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.Subject.Returns("subject-1");
        currentUser.IsAdmin.Returns(isAdmin);
        currentUser.IsSiteAdmin.Returns(isSiteAdmin);
        currentUser.IsGlobalAdmin.Returns(isGlobalAdmin);
        currentUser.SiteId.Returns(siteId);
        return currentUser;
    }
}
