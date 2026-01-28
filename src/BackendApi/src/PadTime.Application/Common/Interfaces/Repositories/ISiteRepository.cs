using PadTime.Domain.Booking;

namespace PadTime.Application.Common.Interfaces.Repositories;

public interface ISiteRepository
{
    Task<Site?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Site?> GetByIdWithSchedulesAsync(Guid id, int year, CancellationToken cancellationToken = default);
    Task<List<Site>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Site site, CancellationToken cancellationToken = default);
}
