using MediatR;
using PadTime.Application.Common.Models;
using PadTime.Domain.Common;
using PadTime.Domain.Members;

namespace PadTime.Application.Admin.Queries.GetMembers;

public sealed record GetMembersQuery(
    int Page,
    int PageSize,
    MemberCategory? Category = null,
    bool? IsActive = null,
    string? Search = null) : IRequest<Result<PagedResult<MemberListItemDto>>>;

public sealed record MemberListItemDto(
    Guid Id,
    string Subject,
    string Matricule,
    MemberCategory Category,
    Guid? SiteId,
    string? SiteName,
    bool IsActive,
    DateTime CreatedAtUtc,
    int MatchCount,
    int DebtAmountCents);
