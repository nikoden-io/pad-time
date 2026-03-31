// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Queries.GetCourtById;

/// <summary>
/// Query to retrieve a single court by its unique identifier.
/// </summary>
/// <param name="CourtId">Unique identifier of the court.</param>
public sealed record GetCourtByIdQuery(Guid CourtId) : IRequest<Result<CourtByIdDto>>;

/// <summary>
/// DTO representing detailed court information including its site affiliation and status.
/// </summary>
public sealed record CourtByIdDto(
    Guid CourtId,
    Guid SiteId,
    string Label,
    bool IsActive,
    DateTime CreatedAtUtc);