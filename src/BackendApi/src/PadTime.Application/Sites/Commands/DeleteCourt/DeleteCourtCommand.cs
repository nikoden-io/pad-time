using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.DeleteCourt;

public sealed record DeleteCourtCommand(
    Guid SiteId,
    Guid CourtId) : IRequest<Result>;
