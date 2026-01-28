using FluentAssertions;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;
using Xunit;

namespace PadTime.Domain.Tests.Booking;

public class MatchTests
{
    private readonly DateTime _utcNow = new(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);
    private readonly Guid _siteId = Guid.NewGuid();
    private readonly Guid _courtId = Guid.NewGuid();
    private readonly Guid _organizerId = Guid.NewGuid();

    private Match CreateMatch(PadMatchType type = PadMatchType.Public)
    {
        var startAt = _utcNow.AddDays(7);
        var endAt = startAt.AddMinutes(90);
        return Match.Create(_siteId, _courtId, _organizerId, startAt, endAt, type, _utcNow).Value;
    }

    [Fact]
    public void CreateWithValidDataCreatesMatchWithOrganizer()
    {
        // Act
        var match = CreateMatch(PadMatchType.Private);

        // Assert
        match.SiteId.Should().Be(_siteId);
        match.CourtId.Should().Be(_courtId);
        match.OrganizerId.Should().Be(_organizerId);
        match.Type.Should().Be(PadMatchType.Private);
        match.Status.Should().Be(MatchStatus.Private);
        match.Participants.Should().HaveCount(1);
        match.Participants[0].Role.Should().Be(ParticipantRole.Organizer);
    }

    [Fact]
    public void CreatePublicMatchHasPublicStatus()
    {
        // Act
        var match = CreateMatch(PadMatchType.Public);

        // Assert
        match.Status.Should().Be(MatchStatus.Public);
    }

    [Fact]
    public void CreateWithStartInPastReturnsFailure()
    {
        // Arrange
        var pastStart = _utcNow.AddHours(-1);
        var pastEnd = pastStart.AddMinutes(90);

        // Act
        var result = Match.Create(_siteId, _courtId, _organizerId, pastStart, pastEnd, PadMatchType.Public, _utcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void JoinPublicWhenPublicAddsParticipant()
    {
        // Arrange
        var match = CreateMatch(PadMatchType.Public);
        var newMemberId = Guid.NewGuid();

        // Act
        var result = match.JoinPublic(newMemberId, _utcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        match.Participants.Should().HaveCount(2);
        result.Value.PaymentStatus.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public void JoinPublicWhenPrivateReturnsFailure()
    {
        // Arrange
        var match = CreateMatch(PadMatchType.Private);
        var newMemberId = Guid.NewGuid();

        // Act
        var result = match.JoinPublic(newMemberId, _utcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Code.Should().Be(DomainErrors.Booking.MatchNotPublic.Code);
    }

    [Fact]
    public void JoinPublicWhenFullReturnsFailure()
    {
        // Arrange
        var match = CreateMatch(PadMatchType.Public);

        // Fill the match
        for (var i = 0; i < 3; i++)
        {
            var participant = match.JoinPublic(Guid.NewGuid(), _utcNow);
            match.ConfirmPayment(participant.Value.Id, _utcNow);
        }
        match.ConfirmPayment(match.Participants[0].Id, _utcNow);

        // Act
        var result = match.JoinPublic(Guid.NewGuid(), _utcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Code.Should().Be(DomainErrors.Booking.MatchFull.Code);
    }

    [Fact]
    public void JoinPublicAlreadyParticipantReturnsFailure()
    {
        // Arrange
        var match = CreateMatch(PadMatchType.Public);
        var memberId = Guid.NewGuid();
        match.JoinPublic(memberId, _utcNow);

        // Act
        var result = match.JoinPublic(memberId, _utcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Code.Should().Be(DomainErrors.Booking.AlreadyParticipant.Code);
    }

    [Fact]
    public void ConfirmPaymentTransitionsToFullWhen4Paid()
    {
        // Arrange
        var match = CreateMatch(PadMatchType.Public);

        // Add 3 more participants
        var participants = new List<Participant> { match.Participants[0] };
        for (var i = 0; i < 3; i++)
        {
            var result = match.JoinPublic(Guid.NewGuid(), _utcNow);
            participants.Add(result.Value);
        }

        // Act - pay all 4
        foreach (var p in participants)
        {
            match.ConfirmPayment(p.Id, _utcNow);
        }

        // Assert
        match.Status.Should().Be(MatchStatus.Full);
        match.GetPaidParticipantCount().Should().Be(4);
    }

    [Fact]
    public void TransitionToPublicAtDeadlinePrivateIncompleteBecomesPublic()
    {
        // Arrange
        var match = CreateMatch(PadMatchType.Private);

        // Act
        var result = match.TransitionToPublicAtDeadline(_utcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        match.Type.Should().Be(PadMatchType.Public);
        match.Status.Should().Be(MatchStatus.Public);
    }

    [Fact]
    public void CancelWhenNotLockedSucceeds()
    {
        // Arrange
        var match = CreateMatch(PadMatchType.Public);

        // Act
        var result = match.Cancel(_utcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        match.Status.Should().Be(MatchStatus.Cancelled);
    }

    [Fact]
    public void CancelWhenCompletedReturnsFailure()
    {
        // Arrange
        var match = CreateMatch(PadMatchType.Public);
        match.Lock(_utcNow);
        match.Complete(_utcNow);

        // Act
        var result = match.Cancel(_utcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void LockWithIncompletePlayersRaisesDebtEvent()
    {
        // Arrange
        var match = CreateMatch(PadMatchType.Public);
        match.ConfirmPayment(match.Participants[0].Id, _utcNow); // Only organizer paid

        // Act
        match.Lock(_utcNow);

        // Assert
        match.Status.Should().Be(MatchStatus.Locked);
        match.DomainEvents.Should().Contain(e => e.GetType().Name == "MatchIncompleteEvent");
    }
}
