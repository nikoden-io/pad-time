using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;

namespace PadTime.Application.Billing.Queries.GetPayment;

public sealed class GetPaymentQueryHandler : IRequestHandler<GetPaymentQuery, Result<PaymentDto>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly ICurrentUser _currentUser;

    public GetPaymentQueryHandler(
        IPaymentRepository paymentRepository,
        IMemberRepository memberRepository,
        ICurrentUser currentUser)
    {
        _paymentRepository = paymentRepository;
        _memberRepository = memberRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<PaymentDto>> Handle(GetPaymentQuery request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken);
        if (payment is null)
            return DomainErrors.Billing.PaymentNotFound;

        // Authorization: owner or admin
        if (!_currentUser.IsAdmin)
        {
            var member = await _memberRepository.GetBySubjectAsync(_currentUser.Subject, cancellationToken);
            if (member is null || member.Id != payment.MemberId)
                return DomainErrors.Billing.PaymentNotFound;
        }

        return new PaymentDto(
            payment.Id,
            payment.MatchId,
            payment.MemberId,
            payment.AmountCents,
            payment.State.ToString().ToLowerInvariant(),
            payment.CreatedAtUtc);
    }
}
