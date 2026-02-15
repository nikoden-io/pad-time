using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Queries.GetCourtById;

public sealed record GetCourtByIdQuery(Guid CourtId) : IRequest<Result<CourtByIdDto>>;

public sealed record CourtByIdDto(
    Guid CourtId,
    Guid SiteId,
    string Label,
    bool IsActive,
    DateTime CreatedAtUtc);
