using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Booking.Commands.JoinMatch;

public sealed record JoinMatchCommand(
    Guid MatchId,
    string IdempotencyKey) : IRequest<Result<JoinMatchResult>>;

public sealed record JoinMatchResult(
    Guid PaymentId,
    string Status);
