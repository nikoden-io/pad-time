// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Site;

namespace PadTime.Application.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for court persistence operations.
/// </summary>
public interface ICourtRepository
{
    /// <summary>
    /// Retrieves a court by its unique identifier, or <c>null</c> if not found.
    /// </summary>
    Task<Court?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all courts belonging to the specified site.
    /// </summary>
    Task<List<Court>> GetBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default);
}