using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;

namespace PadTime.Application.Booking.Commands.CancelMatch;

public sealed class CancelMatchCommandHandler : IRequestHandler<CancelMatchCommand, Result>
{
    private readonly IMatchRepository _matchRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public CancelMatchCommandHandler(
        IMatchRepository matchRepository,
        IMemberRepository memberRepository,
        ICurrentUser currentUser,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _matchRepository = matchRepository;
        _memberRepository = memberRepository;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CancelMatchCommand request, CancellationToken cancellationToken)
    {
        var utcNow = _dateTimeProvider.UtcNow;

        var match = await _matchRepository.GetByIdWithParticipantsAsync(request.MatchId, cancellationToken);
        if (match is null)
            return DomainErrors.Booking.MatchNotFound;

        // Authorization check
        if (!_currentUser.IsAdmin)
        {
            // Non-admin: must be organizer and match not locked
            var member = await _memberRepository.GetBySubjectAsync(_currentUser.Subject, cancellationToken);
            if (member is null || match.OrganizerId != member.Id)
                return DomainErrors.Booking.NotOrganizer;

            if (match.Status == MatchStatus.Locked)
                return DomainErrors.Booking.MatchLocked;
        }
        else if (_currentUser.IsSiteAdmin && !_currentUser.IsGlobalAdmin)
        {
            // Site admin: can only cancel matches in their site
            if (_currentUser.SiteId != match.SiteId)
                return DomainErrors.Booking.SiteScopeViolation;
        }

        var result = match.Cancel(utcNow);
        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
