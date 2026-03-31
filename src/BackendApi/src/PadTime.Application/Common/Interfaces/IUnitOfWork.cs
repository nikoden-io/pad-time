// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
namespace PadTime.Application.Common.Interfaces;

/// <summary>
/// Unit of Work pattern for transaction management.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all pending changes to the data store and returns the number of affected entries.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}