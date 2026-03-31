// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Application.Common.Models;
using PadTime.Domain.Common;
using PadTime.Domain.Members;

namespace PadTime.Application.Admin.Queries.GetMembers;

/// <summary>
/// Query to retrieve a paginated list of members with optional filtering by category, status, and search term.
/// </summary>
/// <param name="Page">Page number (1-based).</param>
/// <param name="PageSize">Number of items per page.</param>
/// <param name="Category">Optional filter by member category.</param>
/// <param name="IsActive">Optional filter by active status.</param>
/// <param name="Search">Optional free-text search on member subject or matricule.</param>
public sealed record GetMembersQuery(
    int Page,
    int PageSize,
    MemberCategory? Category = null,
    bool? IsActive = null,
    string? Search = null) : IRequest<Result<PagedResult<MemberListItemDto>>>;

/// <summary>
/// Summary DTO for a member in a paginated list, including match count and outstanding debt.
/// </summary>
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