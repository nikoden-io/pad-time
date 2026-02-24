using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Billing;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;
using PadTime.Domain.Members;

namespace PadTime.Application.Booking.Commands.JoinMatch;

public sealed class JoinMatchCommandHandler : IRequestHandler<JoinMatchCommand, Result<JoinMatchResult>>
{
    private readonly IMatchRepository _matchRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public JoinMatchCommandHandler(
        IMatchRepository matchRepository,
        IMemberRepository memberRepository,
        IPaymentRepository paymentRepository,
        ICurrentUser currentUser,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _matchRepository = matchRepository;
        _memberRepository = memberRepository;
        _paymentRepository = paymentRepository;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<JoinMatchResult>> Handle(JoinMatchCommand request, CancellationToken cancellationToken)
    {
        var utcNow = _dateTimeProvider.UtcNow;

        // Check idempotency - return existing payment if found
        var existingPayment = await _paymentRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);
        if (existingPayment is not null)
        {
            return new JoinMatchResult(existingPayment.Id, existingPayment.State.ToString().ToLowerInvariant());
        }

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

        // Get match with participants
        var match = await _matchRepository.GetByIdWithParticipantsAsync(request.MatchId, cancellationToken);
        if (match is null)
            return DomainErrors.Booking.MatchNotFound;

        // Join match (validates public status and available spots)
        var joinResult = match.JoinPublic(member.Id, utcNow);
        if (joinResult.IsFailure)
            return joinResult.PadTimeError;

        var participant = joinResult.Value;

        // Create payment
        var paymentResult = Payment.Create(
            match.Id,
            member.Id,
            participant.Id,
            Match.PricePerParticipantCents,
            PaymentPurpose.MatchParticipation,
            request.IdempotencyKey,
            utcNow);

        if (paymentResult.IsFailure)
            return paymentResult.PadTimeError;

        var payment = paymentResult.Value;

        // Simulate successful payment (mock)
        payment.MarkAsPaid(utcNow);

        // Confirm payment on match
        match.ConfirmPayment(participant.Id, utcNow);

        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new JoinMatchResult(payment.Id, payment.State.ToString().ToLowerInvariant());
    }
}
