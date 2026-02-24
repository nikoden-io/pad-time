using Microsoft.EntityFrameworkCore;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Billing;

namespace PadTime.Infrastructure.Persistence.Repositories;

public sealed class OrganizerDebtRepository : IOrganizerDebtRepository
{
    private readonly PadTimeDbContext _context;

    public OrganizerDebtRepository(PadTimeDbContext context)
    {
        _context = context;
    }

    public async Task<OrganizerDebt?> GetByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        return await _context.OrganizerDebts
            .FirstOrDefaultAsync(d => d.MemberId == memberId, cancellationToken);
    }

    public async Task AddAsync(OrganizerDebt debt, CancellationToken cancellationToken = default)
    {
        await _context.OrganizerDebts.AddAsync(debt, cancellationToken);
    }
}
