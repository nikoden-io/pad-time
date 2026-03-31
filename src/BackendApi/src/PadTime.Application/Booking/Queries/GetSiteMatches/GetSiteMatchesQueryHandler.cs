// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;

namespace PadTime.Application.Booking.Queries.GetSiteMatches;

/// <summary>
/// Handles <see cref="GetSiteMatchesQuery"/> by fetching matches for a site with date filtering.
/// Enforces site-admin scope so admins can only view matches in their own site.
/// </summary>
public sealed class GetSiteMatchesQueryHandler : IRequestHandler<GetSiteMatchesQuery, Result<IReadOnlyList<SiteMatchDto>>>
{
    private readonly IMatchRepository _matchRepository;
    private readonly ICurrentUser _currentUser;

    public GetSiteMatchesQueryHandler(IMatchRepository matchRepository, ICurrentUser currentUser)
    {
        _matchRepository = matchRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<SiteMatchDto>>> Handle(
        GetSiteMatchesQuery request,
        CancellationToken cancellationToken)
    {
        // site admins can only see their own site
        if (_currentUser.IsSiteAdmin && !_currentUser.IsGlobalAdmin
            && _currentUser.SiteId != request.SiteId)
            return DomainErrors.Booking.MatchNotFound;

        var matches = await _matchRepository.GetBySiteIdAsync(
            request.SiteId,
            request.FromUtc,
            request.ToUtc,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = matches
            .Select(m => new SiteMatchDto(
                m.Id,
                m.SiteId,
                m.CourtId,
                m.StartAtUtc,
                m.EndAtUtc,
                m.Type.ToString().ToLowerInvariant(),
                m.Status.ToString().ToLowerInvariant(),
                m.OrganizerId,
                Match.TotalPriceCents,
                m.Participants.Count))
            .ToList();

        return Result.Success<IReadOnlyList<SiteMatchDto>>(dtos);
    }
}