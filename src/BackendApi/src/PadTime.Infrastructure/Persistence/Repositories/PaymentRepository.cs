using Microsoft.EntityFrameworkCore;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Billing;

namespace PadTime.Infrastructure.Persistence.Repositories;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly PadTimeDbContext _context;

    public PaymentRepository(PadTimeDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Payment?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public async Task<List<Payment>> GetByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.MemberId == memberId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

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

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await _context.Payments.AddAsync(payment, cancellationToken);
    }
}
