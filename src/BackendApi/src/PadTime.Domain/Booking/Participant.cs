// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Common;

namespace PadTime.Domain.Booking;

/// <summary>
/// Represents a member's participation in a match.
/// Owned by the Match aggregate.
/// </summary>
public sealed class Participant : Entity<Guid>
{
    /// <summary>
    /// The match this participant belongs to.
    /// </summary>
    public Guid MatchId { get; private set; }

    /// <summary>
    /// The member who is participating.
    /// </summary>
    public Guid MemberId { get; private set; }

    /// <summary>
    /// The participant's role (Organizer or Player).
    /// </summary>
    public ParticipantRole Role { get; private set; }

    /// <summary>
    /// Current payment status for this participant's slot.
    /// </summary>
    public PaymentStatus PaymentStatus { get; private set; }

    /// <summary>
    /// When the participant joined the match (UTC).
    /// </summary>
    public DateTime JoinedAtUtc { get; private set; }

    /// <summary>
    /// When the participant's payment was confirmed (UTC). Null if not yet paid.
    /// </summary>
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

    /// <summary>
    /// Indicates whether this participant has confirmed payment.
    /// </summary>
    public bool IsPaid => PaymentStatus == PaymentStatus.Paid;

    /// <summary>
    /// Indicates whether this participant is the match organizer.
    /// </summary>
    public bool IsOrganizer => Role == ParticipantRole.Organizer;
}