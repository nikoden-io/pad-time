using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.ActivateSite;

public sealed record ActivateSiteCommand(Guid SiteId) : IRequest<Result>;