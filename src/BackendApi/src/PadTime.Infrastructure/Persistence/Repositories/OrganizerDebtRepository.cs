// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using Microsoft.EntityFrameworkCore;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Billing;

namespace PadTime.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for <see cref="OrganizerDebt"/> entity data access operations.
/// Provides lookup by member and retrieval of all active (non-zero) debts.
/// </summary>
public sealed class OrganizerDebtRepository : IOrganizerDebtRepository
{
    private readonly PadTimeDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrganizerDebtRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public OrganizerDebtRepository(PadTimeDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<OrganizerDebt?> GetByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        return await _context.OrganizerDebts
            .FirstOrDefaultAsync(d => d.MemberId == memberId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<OrganizerDebt>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.OrganizerDebts
            .Where(d => d.AmountCents > 0)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(OrganizerDebt debt, CancellationToken cancellationToken = default)
    {
        await _context.OrganizerDebts.AddAsync(debt, cancellationToken);
    }
}