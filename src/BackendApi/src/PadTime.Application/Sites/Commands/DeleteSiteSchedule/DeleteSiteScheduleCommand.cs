using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.DeleteSiteSchedule;

public sealed record DeleteSiteScheduleCommand(
    Guid SiteId,
    Guid ScheduleId) : IRequest<Result>;
