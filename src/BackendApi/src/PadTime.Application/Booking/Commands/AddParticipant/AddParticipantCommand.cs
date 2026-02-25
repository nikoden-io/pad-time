using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Booking.Commands.AddParticipant;

public sealed record AddParticipantCommand(Guid MatchId, string Matricule) : IRequest<Result>;
