using PadTime.Application.Common.Models;
using PadTime.Domain.Site;

namespace PadTime.Application.Common.Interfaces.Repositories;

public interface ISiteRepository
{
    Task<Site?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Site?> GetByIdWithSchedulesAndClosuresAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Site>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<Site>> GetPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null, 
        bool? isActive = null, 
        string? city = null, 
        string? country = null, 
        CancellationToken cancellationToken = default);
    Task AddAsync(Site site, CancellationToken cancellationToken = default);
    Task UpdateAsync(Site site, CancellationToken cancellationToken = default);
    Task DeleteAsync(Site site, CancellationToken cancellationToken = default);
    Task<bool> HasActiveBookingsAsync(Guid siteId, CancellationToken cancellationToken = default);
}
