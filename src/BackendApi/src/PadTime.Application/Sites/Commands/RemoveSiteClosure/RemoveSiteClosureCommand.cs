using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.RemoveSiteClosure;

/// <summary>
/// Command to remove a closure (holiday schedule) from a site.
/// </summary>
public sealed record RemoveSiteClosureCommand(
    Guid SiteId,
    Guid ClosureId
) : IRequest<Result>;