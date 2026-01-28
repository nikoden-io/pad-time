using PadTime.Domain.Booking.Events;
using PadTime.Domain.Common;

namespace PadTime.Domain.Booking;

/// <summary>
/// Aggregate root for a padel match booking.
/// Enforces all business rules and state machine transitions.
/// </summary>
public sealed class Match : AggregateRoot<Guid>
{
    public const int MaxParticipants = 4;
    public const int PricePerParticipantCents = 1500; // 15€
    public const int TotalPriceCents = MaxParticipants * PricePerParticipantCents; // 60€

    private readonly List<Participant> _participants = [];

    public Guid SiteId { get; private set; }
    public Guid CourtId { get; private set; }
    public Guid OrganizerId { get; private set; }

    /// <summary>
    /// Match start time (UTC).
    /// </summary>
    public DateTime StartAtUtc { get; private set; }

    /// <summary>
    /// Match end time (UTC).
    /// </summary>
    public DateTime EndAtUtc { get; private set; }

    public PadMatchType Type { get; private set; }
    public MatchStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public IReadOnlyList<Participant> Participants => _participants.AsReadOnly();

    private Match() { } // EF Core

    /// <summary>
    /// Creates a new match.
    /// </summary>
    public static Result<Match> Create(
        Guid siteId,
        Guid courtId,
        Guid organizerId,
        DateTime startAtUtc,
        DateTime endAtUtc,
        PadMatchType type,
        DateTime utcNow)
    {
        if (startAtUtc <= utcNow)
            return DomainErrors.Booking.InvalidTransition;

        if (endAtUtc <= startAtUtc)
            return DomainErrors.Booking.InvalidTransition;

        var match = new Match
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            CourtId = courtId,
            OrganizerId = organizerId,
            StartAtUtc = startAtUtc,
            EndAtUtc = endAtUtc,
            Type = type,
            Status = type == PadMatchType.Private ? MatchStatus.Private : MatchStatus.Public,
            CreatedAtUtc = utcNow
        };

        // Add organizer as first participant
        var organizer = Participant.CreateOrganizer(match.Id, organizerId, utcNow);
        match._participants.Add(organizer);

        match.RaiseDomainEvent(new MatchCreatedEvent(match.Id, siteId, courtId, startAtUtc, type, utcNow));

        return match;
    }

    /// <summary>
    /// Adds a participant to a private match (organizer action).
    /// </summary>
    public Result AddParticipant(Guid memberId, DateTime utcNow)
    {
        if (Status != MatchStatus.Private)
            return DomainErrors.Booking.MatchNotPublic;

        if (_participants.Count >= MaxParticipants)
            return DomainErrors.Booking.MatchFull;

        if (_participants.Any(p => p.MemberId == memberId))
            return DomainErrors.Booking.AlreadyParticipant;

        var participant = Participant.CreatePlayer(Id, memberId, utcNow);
        _participants.Add(participant);
        UpdatedAtUtc = utcNow;

        return Result.Success();
    }

    /// <summary>
    /// Joins a public match (anyone can call this).
    /// Returns the participant that was created for payment processing.
    /// </summary>
    public Result<Participant> JoinPublic(Guid memberId, DateTime utcNow)
    {
        if (GetActiveParticipantCount() >= MaxParticipants || Status == MatchStatus.Full)
            return DomainErrors.Booking.MatchFull;

        if (Status != MatchStatus.Public)
            return DomainErrors.Booking.MatchNotPublic;

        if (_participants.Any(p => p.MemberId == memberId && p.PaymentStatus != PaymentStatus.Excluded))
            return DomainErrors.Booking.AlreadyParticipant;

        var participant = Participant.CreatePlayer(Id, memberId, utcNow);
        participant.MarkAsPending();
        _participants.Add(participant);
        UpdatedAtUtc = utcNow;

        return participant;
    }

    /// <summary>
    /// Confirms payment for a participant.
    /// May transition match to Full status.
    /// </summary>
    public Result ConfirmPayment(Guid participantId, DateTime utcNow)
    {
        var participant = _participants.FirstOrDefault(p => p.Id == participantId);
        if (participant is null)
            return DomainErrors.Member.NotFound;

        if (participant.PaymentStatus == PaymentStatus.Paid)
            return Result.Success(); // Idempotent

        participant.MarkAsPaid(utcNow);
        UpdatedAtUtc = utcNow;

        // Check if match is now full
        if (GetPaidParticipantCount() >= MaxParticipants)
        {
            TransitionTo(MatchStatus.Full, utcNow);
        }

        RaiseDomainEvent(new ParticipantPaidEvent(Id, participant.MemberId, utcNow));

        return Result.Success();
    }

    /// <summary>
    /// Fails payment for a participant in a public match.
    /// </summary>
    public Result FailPayment(Guid participantId, DateTime utcNow)
    {
        var participant = _participants.FirstOrDefault(p => p.Id == participantId);
        if (participant is null)
            return DomainErrors.Member.NotFound;

        participant.MarkAsFailed();
        UpdatedAtUtc = utcNow;

        return Result.Success();
    }

    /// <summary>
    /// J-1 transition: Private match with < 4 players becomes public.
    /// </summary>
    public Result TransitionToPublicAtDeadline(DateTime utcNow)
    {
        if (Status != MatchStatus.Private)
            return DomainErrors.Booking.InvalidTransition;

        if (GetPaidParticipantCount() >= MaxParticipants)
        {
            TransitionTo(MatchStatus.Full, utcNow);
            return Result.Success();
        }

        Type = PadMatchType.Public;
        TransitionTo(MatchStatus.Public, utcNow);

        RaiseDomainEvent(new MatchBecamePublicEvent(Id, utcNow));

        return Result.Success();
    }

    /// <summary>
    /// J-1 transition: Exclude unpaid participants.
    /// </summary>
    public Result ExcludeUnpaidParticipants(DateTime utcNow)
    {
        var unpaid = _participants
            .Where(p => p.PaymentStatus is PaymentStatus.Unpaid or PaymentStatus.Pending or PaymentStatus.Failed)
            .ToList();

        foreach (var participant in unpaid)
        {
            participant.Exclude();
            RaiseDomainEvent(new ParticipantExcludedEvent(Id, participant.MemberId, utcNow));
        }

        UpdatedAtUtc = utcNow;
        return Result.Success();
    }

    /// <summary>
    /// Locks the match when start time is reached.
    /// </summary>
    public Result Lock(DateTime utcNow)
    {
        if (Status is MatchStatus.Locked or MatchStatus.Completed or MatchStatus.Cancelled)
            return DomainErrors.Booking.InvalidTransition;

        TransitionTo(MatchStatus.Locked, utcNow);

        // Calculate debt if match is incomplete
        var paidCount = GetPaidParticipantCount();
        if (paidCount < MaxParticipants)
        {
            var missingSpots = MaxParticipants - paidCount;
            var debtAmount = missingSpots * PricePerParticipantCents;
            RaiseDomainEvent(new MatchIncompleteEvent(Id, OrganizerId, debtAmount, utcNow));
        }

        return Result.Success();
    }

    /// <summary>
    /// Completes the match when end time is reached.
    /// </summary>
    public Result Complete(DateTime utcNow)
    {
        if (Status != MatchStatus.Locked)
            return DomainErrors.Booking.InvalidTransition;

        TransitionTo(MatchStatus.Completed, utcNow);
        RaiseDomainEvent(new MatchCompletedEvent(Id, utcNow));

        return Result.Success();
    }

    /// <summary>
    /// Cancels the match (admin only).
    /// </summary>
    public Result Cancel(DateTime utcNow)
    {
        if (Status is MatchStatus.Completed or MatchStatus.Cancelled)
            return DomainErrors.Booking.InvalidTransition;

        TransitionTo(MatchStatus.Cancelled, utcNow);
        RaiseDomainEvent(new MatchCancelledEvent(Id, utcNow));

        return Result.Success();
    }

    private void TransitionTo(MatchStatus newStatus, DateTime utcNow)
    {
        Status = newStatus;
        UpdatedAtUtc = utcNow;
    }

    public int GetActiveParticipantCount()
    {
        return _participants.Count(p => p.PaymentStatus != PaymentStatus.Excluded);
    }

    public int GetPaidParticipantCount()
    {
        return _participants.Count(p => p.PaymentStatus == PaymentStatus.Paid);
    }

    public bool HasAvailableSpots()
    {
        return GetActiveParticipantCount() < MaxParticipants;
    }

    public Participant? GetOrganizer()
    {
        return _participants.FirstOrDefault(p => p.IsOrganizer);
    }
}
