using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Billing.Commands.PayMatchParticipation;

public sealed record PayMatchParticipationCommand(
    Guid MatchId,
    string IdempotencyKey) : IRequest<Result<PayMatchParticipationResult>>;

public sealed record PayMatchParticipationResult(Guid PaymentId, string Status);
