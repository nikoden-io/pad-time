using MediatR;
using PadTime.Domain.Common;
using PadTime.Domain.Site;

namespace PadTime.Application.Sites.Commands.AddSiteClosure;

/// <summary>
/// Command to add a closure (holiday schedule) to a site.
/// </summary>
public sealed record AddSiteClosureCommand(
    Guid SiteId,
    ClosureType Type,
    ClosureReason Reason,
    string? Description,
    DateOnly StartDate,
    DateOnly? EndDate,
    TimeOnly? ModifiedOpeningTime,
    TimeOnly? ModifiedClosingTime,
    Guid[]? AffectedCourtIds
) : IRequest<Result<Guid>>;