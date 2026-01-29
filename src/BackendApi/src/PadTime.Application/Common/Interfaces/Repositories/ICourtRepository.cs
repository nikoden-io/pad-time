using PadTime.Domain.Booking;

namespace PadTime.Application.Common.Interfaces.Repositories;

public interface ICourtRepository
{
    Task<Court?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Court?>> GetBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default);
}
