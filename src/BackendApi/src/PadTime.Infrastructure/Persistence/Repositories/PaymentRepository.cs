// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using Microsoft.EntityFrameworkCore;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Billing;

namespace PadTime.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for <see cref="Payment"/> entity data access operations including
/// idempotency key lookup, member payment history, and site-scoped revenue queries.
/// </summary>
public sealed class PaymentRepository : IPaymentRepository
{
    private readonly PadTimeDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public PaymentRepository(PadTimeDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Payment?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<Payment>> GetByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.MemberId == memberId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<(Payment Payment, Guid SiteId)>> GetPaidBySiteAndDateRangeAsync(
        Guid? siteId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        var query =
            from p in _context.Payments
            join m in _context.Matches on p.MatchId equals m.Id
            where p.State == PaymentState.Paid
                  && p.CreatedAtUtc >= fromUtc
                  && p.CreatedAtUtc <= toUtc
                  && (siteId == null || m.SiteId == siteId)
            select new { Payment = p, m.SiteId };

        var rows = await query.ToListAsync(cancellationToken);
        return rows.Select(r => (r.Payment, r.SiteId)).ToList();
    }

    /// <inheritdoc />
    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await _context.Payments.AddAsync(payment, cancellationToken);
    }
}