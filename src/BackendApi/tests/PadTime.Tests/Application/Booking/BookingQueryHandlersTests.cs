using FluentAssertions;
using NSubstitute;
using PadTime.Application.Booking.Queries.GetMatch;
using PadTime.Application.Booking.Queries.GetPublicMatches;
using PadTime.Application.Booking.Queries.GetSiteMatches;
using PadTime.Application.Booking.Queries.GetUserMatches;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;
using PadTime.Domain.Members;
using Xunit;

namespace PadTime.Tests.Application.Booking;

public sealed class BookingQueryHandlersTests
{
    private static readonly DateTime Clock = new(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);

    private static Member NewMember(string subject = "sub-1", string matricule = "G1234")
        => Member.Create(subject, matricule, null, Clock).Value;

    private static Match NewMatch(Guid organizerId, PadMatchType type, Guid? siteId = null)
    {
        var start = Clock.AddDays(3);
        return Match.Create(siteId ?? Guid.NewGuid(), Guid.NewGuid(), organizerId,
            start, start.AddMinutes(90), type, Clock).Value;
    }

    // ---------- GetUserMatches ----------
    [Fact]
    public async Task GetUserMatches_WhenMemberMissing_ReturnsMemberNotFound()
    {
        var members = Substitute.For<IMemberRepository>();
        members.GetBySubjectAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Member?)null);
        var handler = new GetUserMatchesQueryHandler(
            Substitute.For<IMatchRepository>(), members, Substitute.For<ICurrentUser>());

        var result = await handler.Handle(new GetUserMatchesQuery(null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Should().Be(DomainErrors.Member.NotFound);
    }

    [Fact]
    public async Task GetUserMatches_WhenMemberHasMatches_MapsDtosWithParticipants()
    {
        var member = NewMember();
        var match = NewMatch(member.Id, PadMatchType.Public);
        var members = Substitute.For<IMemberRepository>();
        members.GetBySubjectAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(member);
        members.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(member);
        var matches = Substitute.For<IMatchRepository>();
        matches.GetByMemberIdAsync(Arg.Any<Guid>(), Arg.Any<DateTime?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<Match> { match });
        var handler = new GetUserMatchesQueryHandler(matches, members, Substitute.For<ICurrentUser>());

        var result = await handler.Handle(new GetUserMatchesQuery(null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value[0].MatchId.Should().Be(match.Id);
        result.Value[0].Participants.Should().ContainSingle()
            .Which.Matricule.Should().Be("G1234");
    }

    // ---------- GetSiteMatches ----------
    [Fact]
    public async Task GetSiteMatches_WhenSiteAdminQueriesOtherSite_ReturnsMatchNotFound()
    {
        var current = Substitute.For<ICurrentUser>();
        current.IsSiteAdmin.Returns(true);
        current.IsGlobalAdmin.Returns(false);
        current.SiteId.Returns(Guid.NewGuid());
        var handler = new GetSiteMatchesQueryHandler(Substitute.For<IMatchRepository>(), current);

        var result = await handler.Handle(
            new GetSiteMatchesQuery(Guid.NewGuid(), null, null, 1, 20), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Should().Be(DomainErrors.Booking.MatchNotFound);
    }

    [Fact]
    public async Task GetSiteMatches_WhenAuthorized_MapsDtos()
    {
        var siteId = Guid.NewGuid();
        var match = NewMatch(Guid.NewGuid(), PadMatchType.Public, siteId);
        var matches = Substitute.For<IMatchRepository>();
        matches.GetBySiteIdAsync(siteId, Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<Match> { match });
        var current = Substitute.For<ICurrentUser>();
        current.IsGlobalAdmin.Returns(true);
        var handler = new GetSiteMatchesQueryHandler(matches, current);

        var result = await handler.Handle(
            new GetSiteMatchesQuery(siteId, null, null, 1, 20), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle().Which.ParticipantCount.Should().Be(1);
    }

    // ---------- GetMatch ----------
    [Fact]
    public async Task GetMatch_WhenMatchMissing_ReturnsMatchNotFound()
    {
        var matches = Substitute.For<IMatchRepository>();
        matches.GetByIdWithParticipantsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Match?)null);
        var handler = new GetMatchQueryHandler(matches, Substitute.For<IMemberRepository>(), Substitute.For<ICurrentUser>());

        var result = await handler.Handle(new GetMatchQuery(Guid.NewGuid()), CancellationToken.None);

        result.PadTimeError.Should().Be(DomainErrors.Booking.MatchNotFound);
    }

    [Fact]
    public async Task GetMatch_WhenPrivateAndCallerNotParticipant_ReturnsMatchNotFound()
    {
        var match = NewMatch(Guid.NewGuid(), PadMatchType.Private);
        var matches = Substitute.For<IMatchRepository>();
        matches.GetByIdWithParticipantsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(match);
        var members = Substitute.For<IMemberRepository>();
        members.GetBySubjectAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(NewMember());
        var current = Substitute.For<ICurrentUser>();
        current.IsAdmin.Returns(false);
        var handler = new GetMatchQueryHandler(matches, members, current);

        var result = await handler.Handle(new GetMatchQuery(match.Id), CancellationToken.None);

        result.PadTimeError.Should().Be(DomainErrors.Booking.MatchNotFound);
    }

    [Fact]
    public async Task GetMatch_WhenPrivateAndCallerIsParticipant_ReturnsMatch()
    {
        var member = NewMember();
        var match = NewMatch(member.Id, PadMatchType.Private);
        var matches = Substitute.For<IMatchRepository>();
        matches.GetByIdWithParticipantsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(match);
        var members = Substitute.For<IMemberRepository>();
        members.GetBySubjectAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(member);
        members.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(member);
        var current = Substitute.For<ICurrentUser>();
        current.IsAdmin.Returns(false);
        var handler = new GetMatchQueryHandler(matches, members, current);

        var result = await handler.Handle(new GetMatchQuery(match.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.MatchId.Should().Be(match.Id);
    }

    [Fact]
    public async Task GetMatch_WhenSiteAdminQueriesOtherSite_ReturnsMatchNotFound()
    {
        var match = NewMatch(Guid.NewGuid(), PadMatchType.Public);
        var matches = Substitute.For<IMatchRepository>();
        matches.GetByIdWithParticipantsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(match);
        var members = Substitute.For<IMemberRepository>();
        members.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(NewMember());
        var current = Substitute.For<ICurrentUser>();
        current.IsAdmin.Returns(true);
        current.IsSiteAdmin.Returns(true);
        current.IsGlobalAdmin.Returns(false);
        current.SiteId.Returns(Guid.NewGuid());
        var handler = new GetMatchQueryHandler(matches, members, current);

        var result = await handler.Handle(new GetMatchQuery(match.Id), CancellationToken.None);

        result.PadTimeError.Should().Be(DomainErrors.Booking.MatchNotFound);
    }

    // ---------- GetPublicMatches ----------
    [Fact]
    public async Task GetPublicMatches_WhenMatchesExist_MapsDtosWithSeats()
    {
        var match = NewMatch(Guid.NewGuid(), PadMatchType.Public);
        var matches = Substitute.For<IMatchRepository>();
        matches.GetPublicMatchesAsync(Arg.Any<Guid?>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<Match> { match });
        var members = Substitute.For<IMemberRepository>();
        members.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(NewMember());
        var handler = new GetPublicMatchesQueryHandler(matches, members, Substitute.For<ICurrentUser>());

        var result = await handler.Handle(
            new GetPublicMatchesQuery(null, null, null, 1, 20), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var dto = result.Value.Should().ContainSingle().Subject;
        dto.MatchId.Should().Be(match.Id);
        dto.Participants.Should().ContainSingle();
        // In this handler ParticipantCount carries the paid-participant count;
        // AvailableSeats is therefore 4 minus that count.
        dto.AvailableSeats.Should().Be(4 - dto.ParticipantCount);
    }
}
