using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.UpdateCourt;

public sealed record UpdateCourtCommand(
    Guid SiteId,
    Guid CourtId,
    string Label) : IRequest<Result>;
