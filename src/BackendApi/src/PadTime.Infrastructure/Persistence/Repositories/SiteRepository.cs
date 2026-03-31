// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using Microsoft.EntityFrameworkCore;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Application.Common.Models;
using PadTime.Domain.Site;

namespace PadTime.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for <see cref="Site"/> entity data access operations including
/// paginated queries with search/filtering, schedule and closure eager loading, and active booking checks.
/// </summary>
public sealed class SiteRepository(PadTimeDbContext context) : ISiteRepository
{
    /// <inheritdoc />
    public async Task<Site?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Sites
            .Include(s => s.Courts)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Site?> GetByIdWithSchedulesAndClosuresAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await context.Sites
            .Include(s => s.Courts)
            .Include(s => s.Schedules)
            .Include(s => s.Closures)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<Site>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await context.Sites
            .Include(s => s.Courts)
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PagedResult<Site>> GetPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null, 
        bool? isActive = null, 
        string? city = null, 
        string? country = null, 
        CancellationToken cancellationToken = default)
    {
        var query = context.Sites
            .Include(s => s.Courts)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(s => 
                EF.Functions.ILike(s.Name, $"%{searchTerm}%") ||
                EF.Functions.ILike(s.City, $"%{searchTerm}%") ||
                EF.Functions.ILike(s.Street, $"%{searchTerm}%"));
        }

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(s => EF.Functions.ILike(s.City, city));
        }

        if (!string.IsNullOrWhiteSpace(country))
        {
            query = query.Where(s => EF.Functions.ILike(s.Country, country));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and ordering
        var sites = await query
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Site>(sites, page, pageSize, totalCount);
    }

    /// <inheritdoc />
    public async Task AddAsync(Site site, CancellationToken cancellationToken = default)
    {
        await context.Sites.AddAsync(site, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Site site, CancellationToken cancellationToken = default)
    {
        context.Sites.Update(site);
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Site site, CancellationToken cancellationToken = default)
    {
        context.Sites.Remove(site);
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<bool> HasActiveBookingsAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        
        // Check if there are any matches (bookings) for courts in this site that are in the future or currently active
        return await context.Matches
            .Join(context.Courts, 
                  match => match.CourtId, 
                  court => court.Id, 
                  (match, court) => new { match, court })
            .AnyAsync(x => x.court.SiteId == siteId && x.match.StartAtUtc >= now, cancellationToken);
    }
}