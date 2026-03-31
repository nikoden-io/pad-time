// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Queries.GetCourtById;

/// <summary>
/// Handles <see cref="GetCourtByIdQuery"/> by fetching the court and mapping it to a DTO.
/// </summary>
public sealed class GetCourtByIdQueryHandler(ICourtRepository courtRepository)
    : IRequestHandler<GetCourtByIdQuery, Result<CourtByIdDto>>
{
    public async Task<Result<CourtByIdDto>> Handle(
        GetCourtByIdQuery request,
        CancellationToken cancellationToken)
    {
        var court = await courtRepository.GetByIdAsync(request.CourtId, cancellationToken);

        if (court is null)
            return Result.Failure<CourtByIdDto>(DomainErrors.Court.NotFound);

        var dto = new CourtByIdDto(
            CourtId: court.Id,
            SiteId: court.SiteId,
            Label: court.Label,
            IsActive: court.IsActive,
            CreatedAtUtc: court.CreatedAtUtc);

        return Result.Success(dto);
    }
}