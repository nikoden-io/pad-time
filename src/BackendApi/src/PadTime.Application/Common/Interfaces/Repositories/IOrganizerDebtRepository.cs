// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Billing;

namespace PadTime.Application.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for organizer debt persistence operations.
/// </summary>
public interface IOrganizerDebtRepository
{
    /// <summary>
    /// Retrieves the organizer debt record for a member, or <c>null</c> if none exists.
    /// </summary>
    Task<OrganizerDebt?> GetByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active (non-zero) organizer debts.
    /// </summary>
    Task<List<OrganizerDebt>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new organizer debt record to the data store.
    /// </summary>
    Task AddAsync(OrganizerDebt debt, CancellationToken cancellationToken = default);
}