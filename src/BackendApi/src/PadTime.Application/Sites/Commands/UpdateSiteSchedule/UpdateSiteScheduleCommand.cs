using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.UpdateSiteSchedule;

public sealed record UpdateSiteScheduleCommand(
    Guid SiteId,
    Guid ScheduleId,
    string Name,
    DateOnly ValidFrom,
    DateOnly? ValidUntil,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime,
    DayOfWeek[]? ApplicableDays,
    int Priority
) : IRequest<Result>;
