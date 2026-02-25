using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;
using PadTime.Domain.Members;
using PadTime.Domain.Site;

namespace PadTime.Application.Booking.Commands.CreateMatch;

/// <summary>
/// Handles the retrieval of matches where the current authenticated user is a participant.
/// Applies authorization scope and maps domain entities to DTOs.
/// </summary>
public sealed class CreateMatchCommandHandler : IRequestHandler<CreateMatchCommand, Result<Guid>>
{
    private readonly IMatchRepository _matchRepository;
    private readonly ISiteRepository _siteRepository;
    private readonly ICourtRepository _courtRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IOrganizerDebtRepository _debtRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMatchCommandHandler(
        IMatchRepository matchRepository,
        ISiteRepository siteRepository,
        ICourtRepository courtRepository,
        IMemberRepository memberRepository,
        IOrganizerDebtRepository debtRepository,
        ICurrentUser currentUser,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _matchRepository = matchRepository;
        _siteRepository = siteRepository;
        _courtRepository = courtRepository;
        _memberRepository = memberRepository;
        _debtRepository = debtRepository;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the query and returns the list of matches for the current user.
    /// </summary>
    /// <param name="request">Query parameters including optional date filter and pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A result containing the list of matches if successful,
    /// or an error if the current user cannot be resolved.
    /// </returns>
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

        // Validate site (with schedules and closures)
        var site = await _siteRepository.GetByIdWithSchedulesAndClosuresAsync(request.SiteId, cancellationToken);
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

        // Check if site/court is closed
        if (site.IsClosedOn(matchDate))
            return DomainErrors.Site.Closed;

        if (site.IsCourtClosedOn(request.CourtId, matchDate))
            return DomainErrors.Site.Closed;

        // Check slot availability (anti double-booking)
        var slotExists = await _matchRepository.ExistsForSlotAsync(request.CourtId, request.StartAtUtc, cancellationToken);
        if (slotExists)
            return DomainErrors.Booking.SlotConflict;

        // Calculate end time (90 minutes)
        var endAtUtc = request.StartAtUtc.AddMinutes(SiteSchedule.SlotDurationMinutes);

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
