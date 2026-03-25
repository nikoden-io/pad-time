using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Billing.Queries.GetPayment;

public sealed record GetPaymentQuery(Guid PaymentId) : IRequest<Result<PaymentDto>>;

public sealed record PaymentDto(
    Guid PaymentId,
    Guid MatchId,
    Guid MemberId,
    int AmountCents,
    string Status,
    DateTime CreatedAtUtc);
