using PadTime.Domain.Booking;

namespace PadTime.Application.Common.Interfaces.Repositories;

public interface IClosureRepository
{
    Task<bool> IsClosedAsync(Guid? siteId, DateOnly checkDate, CancellationToken cancellationToken = default);
    Task<List<Closure>> GetForSiteAndDateRangeAsync(
        Guid siteId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);
}
