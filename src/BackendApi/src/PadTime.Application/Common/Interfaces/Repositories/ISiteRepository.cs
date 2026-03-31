// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Application.Common.Models;
using PadTime.Domain.Site;

namespace PadTime.Application.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for site persistence and query operations.
/// </summary>
public interface ISiteRepository
{
    /// <summary>
    /// Retrieves a site by unique identifier, or <c>null</c> if not found.
    /// </summary>
    Task<Site?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a site by unique identifier with schedules and closures eagerly loaded, or <c>null</c> if not found.
    /// </summary>
    Task<Site?> GetByIdWithSchedulesAndClosuresAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active sites.
    /// </summary>
    Task<List<Site>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of sites with optional filtering by search term, active status, city, and country.
    /// </summary>
    Task<PagedResult<Site>> GetPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null, 
        bool? isActive = null, 
        string? city = null, 
        string? country = null, 
        CancellationToken cancellationToken = default);
    /// <summary>
    /// Adds a new site to the data store.
    /// </summary>
    Task AddAsync(Site site, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing site in the data store.
    /// </summary>
    Task UpdateAsync(Site site, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a site from the data store.
    /// </summary>
    Task DeleteAsync(Site site, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the site has any active or future bookings, preventing deletion.
    /// </summary>
    Task<bool> HasActiveBookingsAsync(Guid siteId, CancellationToken cancellationToken = default);
}