using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;

namespace PadTime.Application.Booking.Commands.AddParticipant;

public sealed class AddParticipantCommandHandler : IRequestHandler<AddParticipantCommand, Result>
{
    private readonly IMatchRepository _matchRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public AddParticipantCommandHandler(
        IMatchRepository matchRepository,
        IMemberRepository memberRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _matchRepository = matchRepository;
        _memberRepository = memberRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddParticipantCommand request, CancellationToken cancellationToken)
    {
        var match = await _matchRepository.GetByIdWithParticipantsAsync(request.MatchId, cancellationToken);
        if (match is null)
            return DomainErrors.Booking.MatchNotFound;

        var organizer = await _memberRepository.GetBySubjectAsync(_currentUser.Subject, cancellationToken);
        if (organizer is null || organizer.Id != match.OrganizerId)
            return DomainErrors.Booking.NotOrganizer;

        var member = await _memberRepository.GetByMatriculeAsync(request.Matricule, cancellationToken);
        if (member is null)
            return DomainErrors.Member.NotFound;

        var result = match.AddParticipant(member.Id, DateTime.UtcNow);
        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
