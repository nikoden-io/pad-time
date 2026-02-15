using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.DeleteSite;

public sealed record DeleteSiteCommand(Guid SiteId) : IRequest<Result>;