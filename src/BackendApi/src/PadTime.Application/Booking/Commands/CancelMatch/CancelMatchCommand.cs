using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Booking.Commands.CancelMatch;

public sealed record CancelMatchCommand(Guid MatchId) : IRequest<Result>;
