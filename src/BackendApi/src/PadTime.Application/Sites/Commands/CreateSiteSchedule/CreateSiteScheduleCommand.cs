using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.CreateSiteSchedule;

public sealed record CreateSiteScheduleCommand(
    Guid SiteId,
    string Name,
    DateOnly ValidFrom,
    DateOnly? ValidUntil,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime,
    DayOfWeek[]? ApplicableDays,
    int Priority
) : IRequest<Result<Guid>>;
