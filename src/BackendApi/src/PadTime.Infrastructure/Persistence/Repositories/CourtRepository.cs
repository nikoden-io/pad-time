using Microsoft.EntityFrameworkCore;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Site;

namespace PadTime.Infrastructure.Persistence.Repositories;

public sealed class CourtRepository(PadTimeDbContext context) : ICourtRepository
{
    public async Task<Court?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Courts
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Court>> GetBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        return await context.Courts
            .Where(c => c.SiteId == siteId)
            .OrderBy(c => c.Label)
            .ToListAsync(cancellationToken);
    }
}
