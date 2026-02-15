using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Queries.GetCourts;

public sealed record GetCourtsQuery(Guid SiteId) : IRequest<Result<List<CourtDto>>>;

public sealed record CourtDto(
    Guid CourtId,
    string Label,
    bool IsActive);
