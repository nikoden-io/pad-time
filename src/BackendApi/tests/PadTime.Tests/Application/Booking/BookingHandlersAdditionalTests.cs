using FluentAssertions;
using NSubstitute;
using PadTime.Application.Booking.Commands.AddParticipant;
using PadTime.Application.Booking.Queries.GetMatch;
using PadTime.Application.Booking.Queries.GetPublicMatches;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;
using PadTime.Domain.Booking;
using PadTime.Domain.Members;
using Xunit;

namespace PadTime.Tests.Application.Booking;

public sealed class BookingHandlersAdditionalTests
{
    [Fact]
    public async Task AddParticipant_WhenCurrentUserIsOrganizer_AddsParticipant()
    {
        var organizer = Member.Create("subject", "G1234", null, DateTime.UtcNow).Value;
        var invitee = Member.Create("subject-2", "G2345", null, DateTime.UtcNow).Value;
        var match = Match.Create(Guid.NewGuid(), Guid.NewGuid(), organizer.Id, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddMinutes(90), PadMatchType.Private, DateTime.UtcNow).Value;
        var matchRepository = Substitute.For<IMatchRepository>();
        var memberRepository = Substitute.For<IMemberRepository>();
        var currentUser = Substitute.For<ICurrentUser>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        currentUser.Subject.Returns("subject");
        matchRepository.GetByIdWithParticipantsAsync(match.Id, Arg.Any<CancellationToken>()).Returns(match);
        memberRepository.GetBySubjectAsync("subject", Arg.Any<CancellationToken>()).Returns(organizer);
        memberRepository.GetByMatriculeAsync("G2345", Arg.Any<CancellationToken>()).Returns(invitee);

        var handler = new AddParticipantCommandHandler(matchRepository, memberRepository, currentUser, unitOfWork);

        var result = await handler.Handle(new AddParticipantCommand(match.Id, "G2345"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        match.Participants.Should().Contain(p => p.MemberId == invitee.Id);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMatch_WhenPrivateMatchAndUserIsNotParticipant_ReturnsNotFound()
    {
        var organizer = Member.Create("organizer", "G1234", null, DateTime.UtcNow).Value;
        var viewer = Member.Create("viewer", "G2345", null, DateTime.UtcNow).Value;
        var match = Match.Create(Guid.NewGuid(), Guid.NewGuid(), organizer.Id, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddMinutes(90), PadMatchType.Private, DateTime.UtcNow).Value;
        var matchRepository = Substitute.For<IMatchRepository>();
        var memberRepository = Substitute.For<IMemberRepository>();
        var currentUser = Substitute.For<ICurrentUser>();

        currentUser.IsAdmin.Returns(false);
        currentUser.Subject.Returns("viewer");
        matchRepository.GetByIdWithParticipantsAsync(match.Id, Arg.Any<CancellationToken>()).Returns(match);
        memberRepository.GetBySubjectAsync("viewer", Arg.Any<CancellationToken>()).Returns(viewer);

        var handler = new GetMatchQueryHandler(matchRepository, memberRepository, currentUser);

        var result = await handler.Handle(new GetMatchQuery(match.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Should().Be(DomainErrors.Booking.MatchNotFound);
    }

    [Fact]
    public async Task GetPublicMatches_WhenMatchesExist_MapsParticipantsAndSeats()
    {
        var currentUser = Substitute.For<ICurrentUser>();
        var matchRepository = Substitute.For<IMatchRepository>();
        var memberRepository = Substitute.For<IMemberRepository>();
        var handler = new GetPublicMatchesQueryHandler(matchRepository, memberRepository, currentUser);
        var now = DateTime.UtcNow;
        var organizer = Member.Create("org", "G1234", null, now).Value;
        var participant = Member.Create("player", "G2345", null, now).Value;
        var match = Match.Create(Guid.NewGuid(), Guid.NewGuid(), organizer.Id, now.AddDays(1), now.AddDays(1).AddMinutes(90), PadMatchType.Public, now).Value;
        var joined = match.JoinPublic(participant.Id, now).Value;
        match.ConfirmPayment(match.GetOrganizer()!.Id, now).IsSuccess.Should().BeTrue();
        match.ConfirmPayment(joined.Id, now).IsSuccess.Should().BeTrue();

        matchRepository.GetPublicMatchesAsync(null, Arg.Any<DateTime>(), Arg.Any<DateTime>(), 1, 20, Arg.Any<CancellationToken>())
            .Returns([match]);
        memberRepository.GetByIdAsync(organizer.Id, Arg.Any<CancellationToken>()).Returns(organizer);
        memberRepository.GetByIdAsync(participant.Id, Arg.Any<CancellationToken>()).Returns(participant);

        var result = await handler.Handle(new GetPublicMatchesQuery(null, now, now.AddDays(30), 1, 20), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value[0].ParticipantCount.Should().Be(2);
        result.Value[0].AvailableSeats.Should().Be(2);
    }
}
