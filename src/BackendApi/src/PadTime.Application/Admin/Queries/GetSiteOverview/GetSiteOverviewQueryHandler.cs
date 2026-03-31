// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;

namespace PadTime.Application.Admin.Queries.GetSiteOverview;

/// <summary>
/// Handles <see cref="GetSiteOverviewQuery"/> by computing alerts for a site, including
/// J-1 unprocessed private matches, upcoming matches with unpaid participants, and active organizer debts.
/// Enforces site-admin scope restrictions.
/// </summary>
public sealed class GetSiteOverviewQueryHandler : IRequestHandler<GetSiteOverviewQuery, Result<SiteOverviewDto>>
{
    private readonly IMatchRepository _matchRepository;
    private readonly IOrganizerDebtRepository _debtRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetSiteOverviewQueryHandler(
        IMatchRepository matchRepository,
        IOrganizerDebtRepository debtRepository,
        ICurrentUser currentUser,
        IDateTimeProvider dateTimeProvider)
    {
        _matchRepository = matchRepository;
        _debtRepository = debtRepository;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<SiteOverviewDto>> Handle(GetSiteOverviewQuery request, CancellationToken cancellationToken)
    {
        // site admin can only see their own site
        if (_currentUser.IsSiteAdmin && !_currentUser.IsGlobalAdmin
            && _currentUser.SiteId != request.SiteId)
            return DomainErrors.Site.NotFound;

        var alerts = new List<SiteAlertDto>();
        var utcNow = _dateTimeProvider.UtcNow;

        // J-1 alerts: private matches scheduled for tomorrow not yet processed
        var tomorrow = utcNow.Date.AddDays(1);
        var j1Matches = await _matchRepository.GetMatchesForDayBeforeProcessingAsync(tomorrow, cancellationToken);
        foreach (var match in j1Matches.Where(m => m.SiteId == request.SiteId && m.Status == MatchStatus.Private))
        {
            alerts.Add(new SiteAlertDto(
                "j1_unprocessed",
                "alerts.j1Unprocessed",
                new { matchId = match.Id, scheduledAt = match.StartAtUtc }));
        }

        // Unpaid participants: upcoming matches with participants still unpaid
        var upcomingMatches = await _matchRepository.GetBySiteIdAsync(
            request.SiteId,
            fromUtc: utcNow,
            toUtc: utcNow.AddDays(7),
            page: 1,
            pageSize: 200,
            cancellationToken);

        foreach (var match in upcomingMatches)
        {
            var unpaidCount = match.Participants.Count(p => p.PaymentStatus == PaymentStatus.Unpaid);
            if (unpaidCount > 0)
            {
                alerts.Add(new SiteAlertDto(
                    "unpaid_participants",
                    "alerts.unpaidParticipants",
                    new { matchId = match.Id, scheduledAt = match.StartAtUtc, unpaidCount }));
            }
        }

        // Active organizer debts
        var activeDebts = await _debtRepository.GetAllActiveAsync(cancellationToken);
        foreach (var debt in activeDebts)
        {
            alerts.Add(new SiteAlertDto(
                "organizer_debt",
                "alerts.organizerDebt",
                new { memberId = debt.MemberId, amountCents = debt.AmountCents }));
        }

        return new SiteOverviewDto(request.SiteId, alerts);
    }
}