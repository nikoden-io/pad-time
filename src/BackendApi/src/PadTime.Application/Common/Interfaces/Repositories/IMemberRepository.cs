using PadTime.Domain.Members;

namespace PadTime.Application.Common.Interfaces.Repositories;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Member?> GetBySubjectAsync(string subject, CancellationToken cancellationToken = default);
    Task<Member?> GetByMatriculeAsync(string matricule, CancellationToken cancellationToken = default);
    Task AddAsync(Member member, CancellationToken cancellationToken = default);
}
