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

        public static readonly PadTimeError MatchNotPrivate =
            new("booking.match_not_private", "This match is not private.");

        public static readonly PadTimeError MatchFull =
            new("booking.match_full", "This match is already full.");

        public static readonly PadTimeError MatchLocked =
            new("booking.match_locked", "This match is locked and cannot be modified.");

        public static readonly PadTimeError AlreadyParticipant =
            new("booking.already_participant", "You are already a participant in this match.");

        public static readonly PadTimeError NotParticipant =
            new("booking.not_participant", "You are not a participant in this match.");

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
        public static readonly PadTimeError NotFound = new(
            "site.not_found",
            "Site not found.");

        public static readonly PadTimeError Closed = new(
            "site.closed",
            "The site is closed on this date.");

        public static readonly PadTimeError InvalidSchedule = new(
            "site.invalid_schedule",
            "Invalid schedule configuration.");

        public static readonly PadTimeError InvalidClosure = new(
            "site.invalid_closure",
            "Invalid closure configuration.");

        public static readonly PadTimeError ScheduleConflict = new(
            "site.schedule_conflict",
            "Schedule conflicts with existing schedule.");

        public static readonly PadTimeError ClosureConflictsWithBookings = new(
            "site.closure_conflicts_with_bookings",
            "Closure conflicts with existing bookings.");

        public static readonly PadTimeError ClosureNotFound = new(
            "site.closure_not_found",
            "Closure not found.");

        public static readonly PadTimeError ScheduleNotFound = new(
            "site.schedule_not_found",
            "Schedule not found.");

        public static readonly PadTimeError CannotDeleteSiteWithActiveBookings = new(
            "site.cannot_delete_with_active_bookings",
            "Cannot delete site with active or future bookings. Consider deactivating instead.");

        public static readonly PadTimeError SiteAlreadyDeactivated = new(
            "site.already_deactivated",
            "Site is already deactivated.");

        public static readonly PadTimeError SiteAlreadyActive = new(
            "site.already_active",
            "Site is already active.");
    }

    public static class SiteSchedule
    {
        public static readonly PadTimeError InvalidDateRange = new(
            "site_schedule.invalid_date_range",
            "Schedule end date must be after start date.");

        public static readonly PadTimeError InvalidTimeRange = new(
            "site_schedule.invalid_time_range",
            "Closing time must be after opening time.");
    }

    public static class Court
    {
        public static readonly PadTimeError NotFound =
            new("court.not_found", "Court not found.");

        public static readonly PadTimeError Inactive =
            new("court.inactive", "This court is not active.");

        public static readonly PadTimeError DuplicateLabel =
            new("court.duplicate_label", "A court with this label already exists for this site.");

        public static readonly PadTimeError CannotDeleteWithActiveBookings =
            new("court.cannot_delete_with_active_bookings", "Cannot delete court with active or future bookings. Consider deactivating the court instead.");
    }
}
