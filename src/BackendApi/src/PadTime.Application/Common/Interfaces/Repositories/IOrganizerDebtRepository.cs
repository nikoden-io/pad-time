using PadTime.Domain.Billing;

namespace PadTime.Application.Common.Interfaces.Repositories;

public interface IOrganizerDebtRepository
{
    Task<OrganizerDebt?> GetByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default);
    Task AddAsync(OrganizerDebt debt, CancellationToken cancellationToken = default);
}
