// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;
using PadTime.Domain.Members;

namespace PadTime.Application.Admin.Queries.GetMemberDetail;

/// <summary>
/// Query to retrieve detailed information about a single member, including match history and debt.
/// </summary>
/// <param name="MemberId">Unique identifier of the member.</param>
public sealed record GetMemberDetailQuery(Guid MemberId) : IRequest<Result<MemberDetailDto>>;

/// <summary>
/// Detailed member information including activity statistics and recent match history.
/// </summary>
public sealed record MemberDetailDto(
    Guid Id,
    string Subject,
    string Matricule,
    string Category,
    Guid? SiteId,
    string? SiteName,
    bool IsActive,
    DateTime CreatedAtUtc,
    int MatchCount,
    int DebtAmountCents,
    int TotalMatchesOrganized,
    int TotalMatchesPlayed,
    IReadOnlyList<MemberMatchDto> RecentMatches);

/// <summary>
/// Summary of a match associated with a member.
/// </summary>
public sealed record MemberMatchDto(
    Guid MatchId,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    string Status,
    bool IsOrganizer);