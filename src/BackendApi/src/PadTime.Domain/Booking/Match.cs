// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Booking.Events;
using PadTime.Domain.Common;

namespace PadTime.Domain.Booking;

/// <summary>
/// Aggregate root for a padel match booking.
/// Enforces the match lifecycle state machine (Private/Public -> Full -> Locked -> Completed/Cancelled)
/// and all business rules around participant management, payments, and the J-1 deadline transition.
/// Invariants: a match has at most 4 participants, the organizer is always the first participant,
/// and state transitions follow the defined state machine.
/// </summary>
public sealed class Match : AggregateRoot<Guid>
{
    /// <summary>
    /// Maximum number of participants in a padel match (4 players).
    /// </summary>
    public const int MaxParticipants = 4;

    /// <summary>
    /// Price per participant in euro cents (15.00 EUR).
    /// </summary>
    public const int PricePerParticipantCents = 1500; // 15€

    /// <summary>
    /// Total price for a full match in euro cents (60.00 EUR).
    /// </summary>
    public const int TotalPriceCents = MaxParticipants * PricePerParticipantCents; // 60€

    private readonly List<Participant> _participants = [];

    /// <summary>
    /// The site where the match takes place.
    /// </summary>
    public Guid SiteId { get; private set; }

    /// <summary>
    /// The court assigned to the match.
    /// </summary>
    public Guid CourtId { get; private set; }

    /// <summary>
    /// The member who created and is responsible for filling the match.
    /// </summary>
    public Guid OrganizerId { get; private set; }

    /// <summary>
    /// Match start time (UTC).
    /// </summary>
    public DateTime StartAtUtc { get; private set; }

    /// <summary>
    /// Match end time (UTC).
    /// </summary>
    public DateTime EndAtUtc { get; private set; }

    /// <summary>
    /// Whether this is a private (organizer-managed) or public (open join) match.
    /// </summary>
    public PadMatchType Type { get; private set; }

    /// <summary>
    /// Current state in the match lifecycle state machine.
    /// </summary>
    public MatchStatus Status { get; private set; }

    /// <summary>
    /// When the match was created (UTC).
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// When the match was last modified (UTC).
    /// </summary>
    public DateTime? UpdatedAtUtc { get; private set; }

    /// <summary>
    /// The list of participants in this match, including the organizer.
    /// </summary>
    public IReadOnlyList<Participant> Participants => _participants.AsReadOnly();

    private Match() { } // EF Core

    /// <summary>
    /// Creates a new match with the organizer automatically added as the first participant.
    /// The initial status is determined by the match type (Private or Public).
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
            return DomainErrors.Booking.MatchNotPrivate;

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
    /// J-1 transition: Private match with less than 4 players becomes public.
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

    /// <summary>
    /// Returns the number of non-excluded participants.
    /// </summary>
    public int GetActiveParticipantCount()
    {
        return _participants.Count(p => p.PaymentStatus != PaymentStatus.Excluded);
    }

    /// <summary>
    /// Returns the number of participants who have confirmed payment.
    /// </summary>
    public int GetPaidParticipantCount()
    {
        return _participants.Count(p => p.PaymentStatus == PaymentStatus.Paid);
    }

    /// <summary>
    /// Indicates whether the match has room for additional participants.
    /// </summary>
    public bool HasAvailableSpots()
    {
        return GetActiveParticipantCount() < MaxParticipants;
    }

    /// <summary>
    /// Returns the participant with the <see cref="ParticipantRole.Organizer"/> role, or <c>null</c> if not found.
    /// </summary>
    public Participant? GetOrganizer()
    {
        return _participants.FirstOrDefault(p => p.IsOrganizer);
    }
}