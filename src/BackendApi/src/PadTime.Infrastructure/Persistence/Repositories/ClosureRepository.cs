using Microsoft.EntityFrameworkCore;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Booking;

namespace PadTime.Infrastructure.Persistence.Repositories;

public sealed class ClosureRepository : IClosureRepository
{
    private readonly PadTimeDbContext _context;

    public ClosureRepository(PadTimeDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsClosedAsync(Guid? siteId, DateOnly checkDate, CancellationToken cancellationToken = default)
    {
        // Check for global closure or site-specific closure
        return await _context.Closures
            .AnyAsync(c =>
                c.Date == checkDate &&
                (c.SiteId == null || c.SiteId == siteId),
                cancellationToken);
    }

    public async Task<List<Closure>> GetForSiteAndDateRangeAsync(
        Guid siteId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.Closures
            .Where(c =>
                c.Date >= fromDate &&
                c.Date <= toDate &&
                (c.SiteId == null || c.SiteId == siteId))
            .OrderBy(c => c.Date)
            .ToListAsync(cancellationToken);
    }
}
