using PadTime.Domain.Booking;

namespace PadTime.Application.Common.Interfaces.Repositories;

public interface IMatchRepository
{
    Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Match?> GetByIdWithParticipantsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a slot is already booked (for anti double-booking).
    /// </summary>
    Task<bool> ExistsForSlotAsync(Guid courtId, DateTime startAtUtc, CancellationToken cancellationToken = default);

    Task<List<Match>> GetPublicMatchesAsync(
        Guid? siteId,
        DateTime fromUtc,
        DateTime toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<List<Match>> GetByMemberIdAsync(
        Guid memberId,
        DateTime? fromUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

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

    Task AddAsync(Match match, CancellationToken cancellationToken = default);
}
