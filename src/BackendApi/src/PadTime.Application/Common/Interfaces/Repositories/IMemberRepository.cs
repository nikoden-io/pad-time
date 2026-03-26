using PadTime.Domain.Members;

namespace PadTime.Application.Common.Interfaces.Repositories;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Member?> GetBySubjectAsync(string subject, CancellationToken cancellationToken = default);
    Task<Member?> GetByMatriculeAsync(string matricule, CancellationToken cancellationToken = default);
    Task AddAsync(Member member, CancellationToken cancellationToken = default);

    Task<(List<Member> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        MemberCategory? category = null,
        bool? isActive = null,
        string? search = null,
        CancellationToken cancellationToken = default);

    Task<int> GetMatchCountAsync(Guid memberId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, int>> GetMatchCountsAsync(IEnumerable<Guid> memberIds, CancellationToken cancellationToken = default);
}
