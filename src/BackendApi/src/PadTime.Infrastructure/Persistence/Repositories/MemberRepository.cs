using Microsoft.EntityFrameworkCore;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Members;

namespace PadTime.Infrastructure.Persistence.Repositories;

public sealed class MemberRepository : IMemberRepository
{
    private readonly PadTimeDbContext _context;

    public MemberRepository(PadTimeDbContext context)
    {
        _context = context;
    }

    public async Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Members
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<Member?> GetBySubjectAsync(string subject, CancellationToken cancellationToken = default)
    {
        return await _context.Members
            .FirstOrDefaultAsync(m => m.Subject == subject, cancellationToken);
    }

    public async Task<Member?> GetByMatriculeAsync(string matricule, CancellationToken cancellationToken = default)
    {
        return await _context.Members
            .FirstOrDefaultAsync(m => m.Matricule.Value == matricule, cancellationToken);
    }

    public async Task AddAsync(Member member, CancellationToken cancellationToken = default)
    {
        await _context.Members.AddAsync(member, cancellationToken);
    }

    public async Task<(List<Member> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        MemberCategory? category = null,
        bool? isActive = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Members.AsQueryable();

        if (category.HasValue)
            query = query.Where(m => m.Category == category.Value);

        if (isActive.HasValue)
            query = query.Where(m => m.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.Matricule.Value.Contains(search));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(m => m.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<int> GetMatchCountAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        return await _context.Participants
            .Where(p => p.MemberId == memberId)
            .Select(p => p.MatchId)
            .Distinct()
            .CountAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, int>> GetMatchCountsAsync(IEnumerable<Guid> memberIds, CancellationToken cancellationToken = default)
    {
        var ids = memberIds.ToList();

        return await _context.Participants
            .Where(p => ids.Contains(p.MemberId))
            .GroupBy(p => p.MemberId)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Select(p => p.MatchId).Distinct().Count(),
                cancellationToken);
    }
}
