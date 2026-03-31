// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Booking;

namespace PadTime.Application.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for match persistence and query operations.
/// </summary>
public interface IMatchRepository
{
    /// <summary>
    /// Retrieves a match by its unique identifier, or <c>null</c> if not found.
    /// </summary>
    Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a match by its unique identifier with participants eagerly loaded, or <c>null</c> if not found.
    /// </summary>
    Task<Match?> GetByIdWithParticipantsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a slot is already booked (for anti double-booking).
    /// </summary>
    Task<bool> ExistsForSlotAsync(Guid courtId, DateTime startAtUtc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a court has any active or future bookings.
    /// </summary>
    Task<bool> HasActiveBookingsForCourtAsync(Guid courtId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves public matches within a date range with optional site filtering and pagination.
    /// </summary>
    Task<List<Match>> GetPublicMatchesAsync(
        Guid? siteId,
        DateTime fromUtc,
        DateTime toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves matches where the specified member is a participant or organizer, with optional date filtering and pagination.
    /// </summary>
    Task<List<Match>> GetByMemberIdAsync(
        Guid memberId,
        DateTime? fromUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves matches for a site within an optional date range, with pagination.
    /// </summary>
    Task<List<Match>> GetBySiteIdAsync(
        Guid siteId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets matches that need J-1 processing.
    /// </summary>
    Task<List<Match>> GetMatchesForDayBeforeProcessingAsync(
        DateTime targetDateUtc,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets matches that need to be locked (start time reached).
    /// </summary>
    Task<List<Match>> GetMatchesToLockAsync(
        DateTime nowUtc,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets matches that need to be completed (end time reached).
    /// </summary>
    Task<List<Match>> GetMatchesToCompleteAsync(
        DateTime nowUtc,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new match to the data store.
    /// </summary>
    Task AddAsync(Match match, CancellationToken cancellationToken = default);
}