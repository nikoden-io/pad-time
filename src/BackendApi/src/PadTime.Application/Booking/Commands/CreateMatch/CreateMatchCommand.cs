using MediatR;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;

namespace PadTime.Application.Booking.Commands.CreateMatch;

public sealed record CreateMatchCommand(
    Guid SiteId,
    Guid CourtId,
    DateTime StartAtUtc,
    PadMatchType Type,
    List<string>? PrivateParticipantMatricules = null) : IRequest<Result<Guid>>;
