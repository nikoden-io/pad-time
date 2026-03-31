// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using PadTime.Domain.Members;

namespace PadTime.Application.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for member persistence and query operations.
/// </summary>
public interface IMemberRepository
{
    /// <summary>
    /// Retrieves a member by unique identifier, or <c>null</c> if not found.
    /// </summary>
    Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a member by their OIDC subject identifier, or <c>null</c> if not found.
    /// </summary>
    Task<Member?> GetBySubjectAsync(string subject, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a member by their business matricule, or <c>null</c> if not found.
    /// </summary>
    Task<Member?> GetByMatriculeAsync(string matricule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new member to the data store.
    /// </summary>
    Task AddAsync(Member member, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of members with optional filtering by category, active status, and search term.
    /// </summary>
    Task<(List<Member> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        MemberCategory? category = null,
        bool? isActive = null,
        string? search = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total match count for a single member.
    /// </summary>
    Task<int> GetMatchCountAsync(Guid memberId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets match counts for multiple members in a single query, keyed by member identifier.
    /// </summary>
    Task<Dictionary<Guid, int>> GetMatchCountsAsync(IEnumerable<Guid> memberIds, CancellationToken cancellationToken = default);
}