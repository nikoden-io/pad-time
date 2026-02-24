using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;

namespace PadTime.Application.Booking.Queries.GetMatch;

public sealed class GetMatchQueryHandler : IRequestHandler<GetMatchQuery, Result<MatchDto>>
{
    private readonly IMatchRepository _matchRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly ICurrentUser _currentUser;

    public GetMatchQueryHandler(
        IMatchRepository matchRepository,
        IMemberRepository memberRepository,
        ICurrentUser currentUser)
    {
        _matchRepository = matchRepository;
        _memberRepository = memberRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<MatchDto>> Handle(GetMatchQuery request, CancellationToken cancellationToken)
    {
        var match = await _matchRepository.GetByIdWithParticipantsAsync(request.MatchId, cancellationToken);
        if (match is null)
            return DomainErrors.Booking.MatchNotFound;

        // Authorization: private matches visible only to participants and admins
        if (match.Type == PadMatchType.Private && !_currentUser.IsAdmin)
        {
            var member = await _memberRepository.GetBySubjectAsync(_currentUser.Subject, cancellationToken);
            if (member is null)
                return DomainErrors.Booking.MatchNotFound;

            var isParticipant = match.Participants.Any(p => p.MemberId == member.Id);
            if (!isParticipant)
                return DomainErrors.Booking.MatchNotFound;
        }

        // Site admin scope check
        if (_currentUser.IsSiteAdmin && !_currentUser.IsGlobalAdmin && _currentUser.SiteId != match.SiteId)
            return DomainErrors.Booking.MatchNotFound;

        // Map participants with matricules
        var participantDtos = new List<ParticipantDto>();
        foreach (var p in match.Participants)
        {
            var participantMember = await _memberRepository.GetByIdAsync(p.MemberId, cancellationToken);
            participantDtos.Add(new ParticipantDto(
                p.MemberId,
                participantMember?.Matricule.Value ?? "Unknown",
                p.Role.ToString().ToLowerInvariant(),
                p.PaymentStatus.ToString().ToLowerInvariant()));
        }

        return new MatchDto(
            match.Id,
            match.SiteId,
            match.CourtId,
            match.StartAtUtc,
            match.EndAtUtc,
            match.Type.ToString().ToLowerInvariant(),
            match.Status.ToString().ToLowerInvariant(),
            match.OrganizerId,
            Match.TotalPriceCents,
            participantDtos);
    }
}
