using Microsoft.EntityFrameworkCore;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Booking;

namespace PadTime.Infrastructure.Persistence.Repositories;

public sealed class SiteRepository(PadTimeDbContext context) : ISiteRepository
{
    public async Task<Site?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Sites
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Site?> GetByIdWithSchedulesAsync(Guid id, int year, CancellationToken cancellationToken = default)
    {
        return await context.Sites
            .Include(s => s.Schedules.Where(sc => sc.Year == year))
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<List<Site>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await context.Sites
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Site site, CancellationToken cancellationToken = default)
    {
        await context.Sites.AddAsync(site, cancellationToken);
    }
}
