// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using Microsoft.EntityFrameworkCore;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Site;

namespace PadTime.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for <see cref="Court"/> entity data access operations.
/// </summary>
public sealed class CourtRepository(PadTimeDbContext context) : ICourtRepository
{
    /// <inheritdoc />
    public async Task<Court?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Courts
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<Court>> GetBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        return await context.Courts
            .Where(c => c.SiteId == siteId)
            .OrderBy(c => c.Label)
            .ToListAsync(cancellationToken);
    }
}