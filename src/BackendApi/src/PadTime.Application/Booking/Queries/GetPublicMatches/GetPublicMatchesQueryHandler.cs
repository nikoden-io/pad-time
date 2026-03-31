// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;

namespace PadTime.Application.Booking.Queries.GetPublicMatches;

/// <summary>
/// Handles <see cref="GetPublicMatchesQuery"/> by fetching public matches within the date range,
/// computing paid participant counts and available seats for each match.
/// </summary>
public sealed class GetPublicMatchesQueryHandler
    : IRequestHandler<GetPublicMatchesQuery, Result<IReadOnlyList<PublicMatchDto>>>
{
    private readonly IMatchRepository _matchRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly ICurrentUser _currentUser;

    public GetPublicMatchesQueryHandler(
        IMatchRepository matchRepository,
        IMemberRepository memberRepository,
        ICurrentUser currentUser)
    {
        _matchRepository = matchRepository;
        _memberRepository = memberRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<PublicMatchDto>>> Handle(
        GetPublicMatchesQuery request,
        CancellationToken cancellationToken)
    {
        var effectiveFrom = request.FromUtc ?? DateTime.UtcNow;
        var effectiveTo = request.ToUtc ?? DateTime.UtcNow.AddDays(30);

        var matches = await _matchRepository.GetPublicMatchesAsync(
            request.SiteId,
            effectiveFrom,
            effectiveTo,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = new List<PublicMatchDto>(matches.Count);

        foreach (var match in matches)
        {
            var participantDtos = new List<ParticipantSummaryDto>(match.Participants.Count);

            foreach (var p in match.Participants)
            {
                var member = await _memberRepository.GetByIdAsync(p.MemberId, cancellationToken);
                participantDtos.Add(new ParticipantSummaryDto(
                    p.MemberId,
                    member?.Matricule.Value ?? "Unknown",
                    p.Role.ToString().ToLowerInvariant(),
                    p.PaymentStatus.ToString().ToLowerInvariant()));
            }

            var paidCount = match.Participants
                .Count(p => p.PaymentStatus == PaymentStatus.Paid);

            dtos.Add(new PublicMatchDto(
                match.Id,
                match.SiteId,
                match.CourtId,
                match.StartAtUtc,
                match.EndAtUtc,
                match.Status.ToString().ToLowerInvariant(),
                match.OrganizerId,
                Match.TotalPriceCents,
                paidCount,
                4 - paidCount,
                participantDtos));
        }

        return Result.Success<IReadOnlyList<PublicMatchDto>>(dtos);
    }
}