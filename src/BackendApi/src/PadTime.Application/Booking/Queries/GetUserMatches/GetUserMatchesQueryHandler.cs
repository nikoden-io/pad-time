// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;

namespace PadTime.Application.Booking.Queries.GetUserMatches;

/// <summary>
/// Handles <see cref="GetUserMatchesQuery"/> by retrieving matches where the current authenticated user
/// is a participant or organizer, and mapping them to DTOs with participant details.
/// </summary>
public sealed class GetUserMatchesQueryHandler : IRequestHandler<GetUserMatchesQuery, Result<IReadOnlyList<UserMatchDto>>>
{
    private readonly IMatchRepository _matchRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly ICurrentUser _currentUser;

    public GetUserMatchesQueryHandler(
        IMatchRepository matchRepository,
        IMemberRepository memberRepository,
        ICurrentUser currentUser)
    {
        _matchRepository = matchRepository;
        _memberRepository = memberRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<UserMatchDto>>> Handle(GetUserMatchesQuery request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetBySubjectAsync(_currentUser.Subject, cancellationToken);
        if (member is null)
            return DomainErrors.Member.NotFound;

        var matches = await _matchRepository.GetByMemberIdAsync(
            member.Id,
            request.FromUtc,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = new List<UserMatchDto>(matches.Count);

        foreach (var match in matches)
        {
            var participantDtos = new List<UserParticipantDto>(match.Participants.Count);

            foreach (var p in match.Participants)
            {
                var participantMember = await _memberRepository.GetByIdAsync(p.MemberId, cancellationToken);

                participantDtos.Add(new UserParticipantDto(
                    p.MemberId,
                    participantMember?.Matricule.Value ?? "Unknown",
                    p.Role.ToString().ToLowerInvariant(),
                    p.PaymentStatus.ToString().ToLowerInvariant()));
            }

            dtos.Add(new UserMatchDto(
                match.Id,
                match.SiteId,
                match.CourtId,
                match.StartAtUtc,
                match.EndAtUtc,
                match.Type.ToString().ToLowerInvariant(),
                match.Status.ToString().ToLowerInvariant(),
                match.OrganizerId,
                Match.TotalPriceCents,
                participantDtos));
        }

        return dtos;
    }
}