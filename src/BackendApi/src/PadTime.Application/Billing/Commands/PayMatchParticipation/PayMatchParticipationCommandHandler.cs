using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Billing;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;

namespace PadTime.Application.Billing.Commands.PayMatchParticipation;

public sealed class PayMatchParticipationCommandHandler
    : IRequestHandler<PayMatchParticipationCommand, Result<PayMatchParticipationResult>>
{
    private readonly IMatchRepository _matchRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public PayMatchParticipationCommandHandler(
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

    public async Task<Result<PayMatchParticipationResult>> Handle(
        PayMatchParticipationCommand request,
        CancellationToken cancellationToken)
    {
        var utcNow = _dateTimeProvider.UtcNow;

        var existingPayment = await _paymentRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);
        if (existingPayment is not null)
            return new PayMatchParticipationResult(existingPayment.Id, existingPayment.State.ToString().ToLowerInvariant());

        var member = await _memberRepository.GetBySubjectAsync(_currentUser.Subject, cancellationToken);
        if (member is null)
            return DomainErrors.Member.NotFound;

        var match = await _matchRepository.GetByIdWithParticipantsAsync(request.MatchId, cancellationToken);
        if (match is null)
            return DomainErrors.Booking.MatchNotFound;

        var participant = match.Participants.FirstOrDefault(p => p.MemberId == member.Id);
        if (participant is null)
            return DomainErrors.Booking.NotParticipant;

        if (participant.PaymentStatus != PaymentStatus.Unpaid)
            return DomainErrors.Billing.PaymentAlreadyProcessed;

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
        payment.MarkAsPaid(utcNow);

        match.ConfirmPayment(participant.Id, utcNow);

        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new PayMatchParticipationResult(payment.Id, payment.State.ToString().ToLowerInvariant());
    }
}
