using PadTime.Domain.Common;

namespace PadTime.Domain.Booking;

/// <summary>
/// Represents a member's participation in a match.
/// Owned by the Match aggregate.
/// </summary>
public sealed class Participant : Entity<Guid>
{
    public Guid MatchId { get; private set; }
    public Guid MemberId { get; private set; }
    public ParticipantRole Role { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public DateTime JoinedAtUtc { get; private set; }
    public DateTime? PaidAtUtc { get; private set; }

    private Participant() { } // EF Core

    internal static Participant CreateOrganizer(Guid matchId, Guid memberId, DateTime utcNow)
    {
        return new Participant
        {
            Id = Guid.NewGuid(),
            MatchId = matchId,
            MemberId = memberId,
            Role = ParticipantRole.Organizer,
            PaymentStatus = PaymentStatus.Unpaid,
            JoinedAtUtc = utcNow
        };
    }

    internal static Participant CreatePlayer(Guid matchId, Guid memberId, DateTime utcNow)
    {
        return new Participant
        {
            Id = Guid.NewGuid(),
            MatchId = matchId,
            MemberId = memberId,
            Role = ParticipantRole.Player,
            PaymentStatus = PaymentStatus.Unpaid,
            JoinedAtUtc = utcNow
        };
    }

    internal void MarkAsPending()
    {
        PaymentStatus = PaymentStatus.Pending;
    }

    internal void MarkAsPaid(DateTime utcNow)
    {
        PaymentStatus = PaymentStatus.Paid;
        PaidAtUtc = utcNow;
    }

    internal void MarkAsFailed()
    {
        PaymentStatus = PaymentStatus.Failed;
    }

    internal void Exclude()
    {
        PaymentStatus = PaymentStatus.Excluded;
    }

    public bool IsPaid => PaymentStatus == PaymentStatus.Paid;
    public bool IsOrganizer => Role == ParticipantRole.Organizer;
}
