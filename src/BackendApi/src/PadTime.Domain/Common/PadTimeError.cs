namespace PadTime.Domain.Common;

/// <summary>
/// Represents a domain error with a machine-readable code and human-readable message.
/// Codes follow the pattern: bounded_context.error_name (e.g., booking.slot_conflict)
/// </summary>
public sealed record PadTimeError(string Code, string Message)
{
    public static readonly PadTimeError None = new(string.Empty, string.Empty);

    public static implicit operator string(PadTimeError padTimeError) => padTimeError.Code;
}

/// <summary>
/// Centralized domain errors organized by bounded context.
/// These codes are used in ProblemDetails responses (RFC 7807).
/// </summary>
public static class DomainErrors
{
    public static class Booking
    {
        public static readonly PadTimeError SlotConflict =
            new("booking.slot_conflict", "This time slot is already booked.");

        public static readonly PadTimeError ReservationWindowDenied =
            new("booking.reservation_window_denied", "You cannot book this far in advance for your member category.");

        public static readonly PadTimeError SiteScopeViolation =
            new("booking.site_scope_violation", "You can only book at your assigned site.");

        public static readonly PadTimeError MatchNotFound =
            new("booking.match_not_found", "Match not found.");

        public static readonly PadTimeError MatchNotPublic =
            new("booking.match_not_public", "This match is not public.");

        public static readonly PadTimeError MatchFull =
            new("booking.match_full", "This match is already full.");

        public static readonly PadTimeError MatchLocked =
            new("booking.match_locked", "This match is locked and cannot be modified.");

        public static readonly PadTimeError AlreadyParticipant =
            new("booking.already_participant", "You are already a participant in this match.");

        public static readonly PadTimeError NotOrganizer =
            new("booking.not_organizer", "Only the organizer can perform this action.");

        public static readonly PadTimeError InvalidTransition =
            new("booking.invalid_transition", "This state transition is not allowed.");
    }

    public static class Billing
    {
        public static readonly PadTimeError OrganizerDebtBlock =
            new("billing.organizer_debt_block", "You have an outstanding debt and cannot create new matches.");

        public static readonly PadTimeError PaymentNotFound =
            new("billing.payment_not_found", "Payment not found.");

        public static readonly PadTimeError IdempotencyConflict =
            new("billing.idempotency_conflict", "A payment with this idempotency key already exists.");

        public static readonly PadTimeError PaymentAlreadyProcessed =
            new("billing.payment_already_processed", "This payment has already been processed.");

        public static readonly PadTimeError InvalidAmount =
            new("billing.invalid_amount", "Payment amount must be positive.");
    }

    public static class Member
    {
        public static readonly PadTimeError NotFound =
            new("member.not_found", "Member not found.");

        public static readonly PadTimeError InvalidMatricule =
            new("member.invalid_matricule", "Invalid matricule format.");

        public static readonly PadTimeError Inactive =
            new("member.inactive", "This member account is inactive.");
    }

    public static class Site
    {
        public static readonly PadTimeError NotFound =
            new("site.not_found", "Site not found.");

        public static readonly PadTimeError Closed =
            new("site.closed", "The site is closed on this date.");

        public static readonly PadTimeError NoSchedule =
            new("site.no_schedule", "No schedule defined for this site and year.");
    }

    public static class Court
    {
        public static readonly PadTimeError NotFound =
            new("court.not_found", "Court not found.");

        public static readonly PadTimeError Inactive =
            new("court.inactive", "This court is not active.");
    }
}
