using Microsoft.EntityFrameworkCore;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Booking;

namespace PadTime.Infrastructure.Persistence.Repositories;

public sealed class CourtRepository : ICourtRepository
{
    private readonly PadTimeDbContext _context;

    public CourtRepository(PadTimeDbContext context)
    {
        _context = context;
    }

    public async Task<Court?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Courts
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Court?>> GetBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        return await _context.Courts
            .Where(c => c.SiteId == siteId)
            .OrderBy(c => c.Label)
            .Select(c => (Court?)c)
            .ToListAsync(cancellationToken);
    }
}
