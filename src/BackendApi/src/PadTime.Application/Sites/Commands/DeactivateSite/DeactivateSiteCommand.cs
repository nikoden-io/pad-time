using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.DeactivateSite;

public sealed record DeactivateSiteCommand(Guid SiteId) : IRequest<Result>;