using FluentAssertions;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;
using Xunit;

namespace PadTime.Tests.Domain.Booking;

public sealed class MatchTests
{
    [Fact]
    public void Create_WithFuturePrivateMatch_CreatesMatchWithOrganizer()
    {
        var now = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);

        var result = Match.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            now.AddDays(2),
            now.AddDays(2).AddMinutes(90),
            PadMatchType.Private,
            now);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(MatchStatus.Private);
        result.Value.Participants.Should().ContainSingle();
        result.Value.Participants[0].IsOrganizer.Should().BeTrue();
        result.Value.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Create_WithPastStartTime_ReturnsInvalidTransition()
    {
        var now = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);

        var result = Match.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            now.AddMinutes(-1),
            now.AddMinutes(89),
            PadMatchType.Public,
            now);

        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Should().Be(DomainErrors.Booking.InvalidTransition);
    }

    [Fact]
    public void JoinPublic_WhenMatchIsPublic_AddsPendingParticipant()
    {
        var match = CreateMatch(PadMatchType.Public);
        var now = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc);

        var result = match.JoinPublic(Guid.NewGuid(), now);

        result.IsSuccess.Should().BeTrue();
        result.Value.PaymentStatus.Should().Be(PaymentStatus.Pending);
        match.Participants.Should().HaveCount(2);
        match.Status.Should().Be(MatchStatus.Public);
        match.UpdatedAtUtc.Should().Be(now);
    }

    [Fact]
    public void JoinPublic_WhenMemberAlreadyActiveParticipant_ReturnsAlreadyParticipant()
    {
        var match = CreateMatch(PadMatchType.Public);
        var memberId = Guid.NewGuid();

        match.JoinPublic(memberId, DateTime.UtcNow).IsSuccess.Should().BeTrue();

        var result = match.JoinPublic(memberId, DateTime.UtcNow.AddMinutes(1));

        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Should().Be(DomainErrors.Booking.AlreadyParticipant);
    }

    [Fact]
    public void AddParticipant_WhenPrivateMatchHasRoom_AddsUnpaidParticipant()
    {
        var match = CreateMatch(PadMatchType.Private);
        var now = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc);

        var result = match.AddParticipant(Guid.NewGuid(), now);

        result.IsSuccess.Should().BeTrue();
        match.Participants.Should().HaveCount(2);
        match.Participants[^1].PaymentStatus.Should().Be(PaymentStatus.Unpaid);
        match.UpdatedAtUtc.Should().Be(now);
    }

    [Fact]
    public void AddParticipant_WhenMatchIsNotPrivate_ReturnsMatchNotPrivate()
    {
        var match = CreateMatch(PadMatchType.Public);

        var result = match.AddParticipant(Guid.NewGuid(), DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Should().Be(DomainErrors.Booking.MatchNotPrivate);
    }

    [Fact]
    public void ConfirmPayment_WhenFourthParticipantPays_TransitionsToFull()
    {
        var match = CreateMatch(PadMatchType.Public);
        var now = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);

        var organizer = match.GetOrganizer()!;
        match.ConfirmPayment(organizer.Id, now).IsSuccess.Should().BeTrue();

        var participantIds = Enumerable.Range(0, 3)
            .Select(index => match.JoinPublic(Guid.NewGuid(), now.AddMinutes(index + 1)).Value.Id)
            .ToList();

        foreach (var participantId in participantIds)
        {
            match.ConfirmPayment(participantId, now.AddMinutes(10)).IsSuccess.Should().BeTrue();
        }

        match.Status.Should().Be(MatchStatus.Full);
        match.GetPaidParticipantCount().Should().Be(4);
        match.DomainEvents.Should().Contain(e => e.GetType().Name == "ParticipantPaidEvent");
    }

    [Fact]
    public void ConfirmPayment_WhenParticipantNotFound_ReturnsMemberNotFound()
    {
        var match = CreateMatch(PadMatchType.Public);

        var result = match.ConfirmPayment(Guid.NewGuid(), DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Should().Be(DomainErrors.Member.NotFound);
    }

    [Fact]
    public void Cancel_WhenMatchIsActive_TransitionsToCancelled()
    {
        var match = CreateMatch(PadMatchType.Private);
        var now = new DateTime(2026, 4, 1, 13, 0, 0, DateTimeKind.Utc);

        var result = match.Cancel(now);

        result.IsSuccess.Should().BeTrue();
        match.Status.Should().Be(MatchStatus.Cancelled);
        match.DomainEvents.Should().Contain(e => e.GetType().Name == "MatchCancelledEvent");
    }

    [Fact]
    public void Lock_WhenMatchIsIncomplete_TransitionsToLockedAndRaisesIncompleteEvent()
    {
        var match = CreateMatch(PadMatchType.Private);
        var organizer = match.GetOrganizer()!;

        match.ConfirmPayment(organizer.Id, DateTime.UtcNow).IsSuccess.Should().BeTrue();

        var result = match.Lock(DateTime.UtcNow.AddDays(1));

        result.IsSuccess.Should().BeTrue();
        match.Status.Should().Be(MatchStatus.Locked);
        match.DomainEvents.Should().Contain(e => e.GetType().Name == "MatchIncompleteEvent");
    }

    [Fact]
    public void Complete_WhenMatchIsLocked_TransitionsToCompleted()
    {
        var match = CreateMatch(PadMatchType.Private);
        match.Lock(DateTime.UtcNow).IsSuccess.Should().BeTrue();

        var result = match.Complete(DateTime.UtcNow.AddMinutes(90));

        result.IsSuccess.Should().BeTrue();
        match.Status.Should().Be(MatchStatus.Completed);
        match.DomainEvents.Should().Contain(e => e.GetType().Name == "MatchCompletedEvent");
    }

    [Fact]
    public void Complete_WhenMatchIsNotLocked_ReturnsInvalidTransition()
    {
        var match = CreateMatch(PadMatchType.Private);

        var result = match.Complete(DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.PadTimeError.Should().Be(DomainErrors.Booking.InvalidTransition);
    }

    private static Match CreateMatch(PadMatchType type)
    {
        var now = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);

        return Match.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            now.AddDays(1),
            now.AddDays(1).AddMinutes(90),
            type,
            now).Value;
    }
}
