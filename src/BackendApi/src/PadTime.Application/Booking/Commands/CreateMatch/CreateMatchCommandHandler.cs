using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;
using PadTime.Domain.Members;

namespace PadTime.Application.Booking.Commands.CreateMatch;

public sealed class CreateMatchCommandHandler : IRequestHandler<CreateMatchCommand, Result<Guid>>
{
    private readonly IMatchRepository _matchRepository;
    private readonly ISiteRepository _siteRepository;
    private readonly ICourtRepository _courtRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IOrganizerDebtRepository _debtRepository;
    private readonly IClosureRepository _closureRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMatchCommandHandler(
        IMatchRepository matchRepository,
        ISiteRepository siteRepository,
        ICourtRepository courtRepository,
        IMemberRepository memberRepository,
        IOrganizerDebtRepository debtRepository,
        IClosureRepository closureRepository,
        ICurrentUser currentUser,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _matchRepository = matchRepository;
        _siteRepository = siteRepository;
        _courtRepository = courtRepository;
        _memberRepository = memberRepository;
        _debtRepository = debtRepository;
        _closureRepository = closureRepository;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateMatchCommand request, CancellationToken cancellationToken)
    {
        var utcNow = _dateTimeProvider.UtcNow;

        // Get or create member
        var member = await _memberRepository.GetBySubjectAsync(_currentUser.Subject, cancellationToken);
        if (member is null)
        {
            var memberResult = Member.Create(
                _currentUser.Subject,
                _currentUser.Matricule,
                _currentUser.SiteId,
                utcNow);

            if (memberResult.IsFailure)
                return memberResult.PadTimeError;

            member = memberResult.Value;
            await _memberRepository.AddAsync(member, cancellationToken);
        }

        if (!member.IsActive)
            return DomainErrors.Member.Inactive;

        // Check debt
        var debt = await _debtRepository.GetByMemberIdAsync(member.Id, cancellationToken);
        if (debt is not null && debt.HasDebt)
            return DomainErrors.Billing.OrganizerDebtBlock;

        // Validate site
        var site = await _siteRepository.GetByIdAsync(request.SiteId, cancellationToken);
        if (site is null)
            return DomainErrors.Site.NotFound;

        // Check site scope for site members
        if (!member.CanBookAtSite(request.SiteId))
            return DomainErrors.Booking.SiteScopeViolation;

        // Check booking window
        var matchDate = DateOnly.FromDateTime(request.StartAtUtc);
        if (!member.CanBookForDate(matchDate, _dateTimeProvider.Today))
            return DomainErrors.Booking.ReservationWindowDenied;

        // Validate court
        var court = await _courtRepository.GetByIdAsync(request.CourtId, cancellationToken);
        if (court is null || court.SiteId != request.SiteId)
            return DomainErrors.Court.NotFound;

        if (!court.IsActive)
            return DomainErrors.Court.Inactive;

        // Check closure
        var isClosed = await _closureRepository.IsClosedAsync(request.SiteId, matchDate, cancellationToken);
        if (isClosed)
            return DomainErrors.Site.Closed;

        // Check slot availability (anti double-booking)
        var slotExists = await _matchRepository.ExistsForSlotAsync(request.CourtId, request.StartAtUtc, cancellationToken);
        if (slotExists)
            return DomainErrors.Booking.SlotConflict;

        // Calculate end time (90 minutes)
        var endAtUtc = request.StartAtUtc.AddMinutes(SiteYearSchedule.SlotDurationMinutes);

        // Create match
        var matchResult = Match.Create(
            request.SiteId,
            request.CourtId,
            member.Id,
            request.StartAtUtc,
            endAtUtc,
            request.Type,
            utcNow);

        if (matchResult.IsFailure)
            return matchResult.PadTimeError;

        var match = matchResult.Value;

        // Add private participants if any
        if (request.Type == PadMatchType.Private && request.PrivateParticipantMatricules is not null)
        {
            foreach (var matricule in request.PrivateParticipantMatricules)
            {
                var participant = await _memberRepository.GetByMatriculeAsync(matricule, cancellationToken);
                if (participant is null)
                    return DomainErrors.Member.NotFound;

                var addResult = match.AddParticipant(participant.Id, utcNow);
                if (addResult.IsFailure)
                    return addResult.PadTimeError;
            }
        }

        await _matchRepository.AddAsync(match, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return match.Id;
    }
}
