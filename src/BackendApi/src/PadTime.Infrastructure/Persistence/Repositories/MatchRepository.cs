using Microsoft.EntityFrameworkCore;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Booking;

namespace PadTime.Infrastructure.Persistence.Repositories;

public sealed class MatchRepository : IMatchRepository
{
    private readonly PadTimeDbContext _context;

    public MatchRepository(PadTimeDbContext context)
    {
        _context = context;
    }

    public async Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Matches
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<Match?> GetByIdWithParticipantsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Matches
            .Include(m => m.Participants)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsForSlotAsync(Guid courtId, DateTime startAtUtc, CancellationToken cancellationToken = default)
    {
        return await _context.Matches
            .AnyAsync(m =>
                m.CourtId == courtId &&
                m.StartAtUtc == startAtUtc &&
                m.Status != MatchStatus.Cancelled,
                cancellationToken);
    }

    public async Task<List<Match>> GetPublicMatchesAsync(
        Guid? siteId,
        DateTime fromUtc,
        DateTime toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Matches
            .Include(m => m.Participants)
            .Where(m => m.Type == PadMatchType.Public)
            .Where(m => m.Status == MatchStatus.Public || m.Status == MatchStatus.Full)
            .Where(m => m.StartAtUtc >= fromUtc && m.StartAtUtc <= toUtc);

        if (siteId.HasValue)
            query = query.Where(m => m.SiteId == siteId.Value);

        return await query
            .OrderBy(m => m.StartAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Match>> GetByMemberIdAsync(
        Guid memberId,
        DateTime? fromUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Matches
            .Include(m => m.Participants)
            .Where(m => m.Participants.Any(p => p.MemberId == memberId));

        if (fromUtc.HasValue)
            query = query.Where(m => m.StartAtUtc >= fromUtc.Value);

        return await query
            .OrderByDescending(m => m.StartAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Match>> GetBySiteIdAsync(
        Guid siteId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Matches
            .Include(m => m.Participants)
            .Where(m => m.SiteId == siteId);

        if (fromUtc.HasValue)
            query = query.Where(m => m.StartAtUtc >= fromUtc.Value);

        if (toUtc.HasValue)
            query = query.Where(m => m.StartAtUtc <= toUtc.Value);

        return await query
            .OrderBy(m => m.StartAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Match>> GetMatchesForDayBeforeProcessingAsync(
        DateTime targetDateUtc,
        CancellationToken cancellationToken = default)
    {
        var dayStart = targetDateUtc.Date;
        var dayEnd = dayStart.AddDays(1);

        return await _context.Matches
            .Include(m => m.Participants)
            .Where(m => m.StartAtUtc >= dayStart && m.StartAtUtc < dayEnd)
            .Where(m => m.Status == MatchStatus.Private || m.Status == MatchStatus.Public)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Match>> GetMatchesToLockAsync(
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        return await _context.Matches
            .Include(m => m.Participants)
            .Where(m => m.StartAtUtc <= nowUtc)
            .Where(m => m.Status != MatchStatus.Locked &&
                       m.Status != MatchStatus.Completed &&
                       m.Status != MatchStatus.Cancelled)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Match>> GetMatchesToCompleteAsync(
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        return await _context.Matches
            .Where(m => m.EndAtUtc <= nowUtc)
            .Where(m => m.Status == MatchStatus.Locked)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Match match, CancellationToken cancellationToken = default)
    {
        await _context.Matches.AddAsync(match, cancellationToken);
    }
}
