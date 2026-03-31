// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;

namespace PadTime.Application.Admin.Queries.GetMemberDetail;

/// <summary>
/// Handles <see cref="GetMemberDetailQuery"/> by assembling detailed member data
/// including match counts, organizer debt, site affiliation, and recent match history.
/// </summary>
public sealed class GetMemberDetailQueryHandler(
    IMemberRepository members,
    IMatchRepository matches,
    IOrganizerDebtRepository debts,
    ISiteRepository sites)
    : IRequestHandler<GetMemberDetailQuery, Result<MemberDetailDto>>
{
    public async Task<Result<MemberDetailDto>> Handle(
        GetMemberDetailQuery request, CancellationToken cancellationToken)
    {
        var member = await members.GetByIdAsync(request.MemberId, cancellationToken);
        if (member is null)
            return DomainErrors.Member.NotFound;

        var matchCount = await members.GetMatchCountAsync(request.MemberId, cancellationToken);

        var debt = await debts.GetByMemberIdAsync(request.MemberId, cancellationToken);
        var debtAmount = debt?.AmountCents ?? 0;

        string? siteName = null;
        if (member.SiteId.HasValue)
        {
            var site = await sites.GetByIdAsync(member.SiteId.Value, cancellationToken);
            siteName = site?.Name;
        }

        // Get recent matches (as participant or organizer)
        var recentMatches = await matches.GetByMemberIdAsync(
            request.MemberId, fromUtc: null, page: 1, pageSize: 5, cancellationToken);

        var organized = recentMatches.Count(m => m.OrganizerId == request.MemberId);

        // For total counts, use all matches not just recent
        var allMatches = await matches.GetByMemberIdAsync(
            request.MemberId, fromUtc: null, page: 1, pageSize: int.MaxValue, cancellationToken);

        var totalOrganized = allMatches.Count(m => m.OrganizerId == request.MemberId);
        var totalPlayed = allMatches.Count(m => m.OrganizerId != request.MemberId);

        var recentMatchDtos = recentMatches
            .OrderByDescending(m => m.StartAtUtc)
            .Take(5)
            .Select(m => new MemberMatchDto(
                m.Id,
                m.StartAtUtc,
                m.EndAtUtc,
                m.Status.ToString(),
                m.OrganizerId == request.MemberId))
            .ToList();

        return new MemberDetailDto(
            member.Id,
            member.Subject,
            member.Matricule.Value,
            member.Category.ToString(),
            member.SiteId,
            siteName,
            member.IsActive,
            member.CreatedAtUtc,
            matchCount,
            debtAmount,
            totalOrganized,
            totalPlayed,
            recentMatchDtos);
    }
}