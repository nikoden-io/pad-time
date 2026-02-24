using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.CreateCourt;

public sealed record CreateCourtCommand(
    Guid SiteId,
    string Label) : IRequest<Result<Guid>>;
