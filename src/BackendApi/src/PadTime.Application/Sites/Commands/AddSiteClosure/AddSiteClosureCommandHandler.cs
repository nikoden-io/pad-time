// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;
using PadTime.Domain.Site;

namespace PadTime.Application.Sites.Commands.AddSiteClosure;

/// <summary>
/// Handler for adding a closure (holiday schedule) to a site.
/// </summary>
public sealed class AddSiteClosureCommandHandler(
    ISiteRepository siteRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<AddSiteClosureCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        AddSiteClosureCommand request,
        CancellationToken cancellationToken)
    {
        var site = await siteRepository.GetByIdWithSchedulesAndClosuresAsync(request.SiteId, cancellationToken);
        if (site == null)
            return DomainErrors.Site.NotFound;

        Result<SiteClosure> closureResult = request.Type switch
        {
            ClosureType.FullDay => site.AddFullDayClosure(
                request.StartDate,
                request.Reason,
                request.Description,
                request.AffectedCourtIds,
                dateTimeProvider.UtcNow),

            ClosureType.Period => site.AddPeriodClosure(
                request.StartDate,
                request.EndDate ?? request.StartDate,
                request.Reason,
                request.Description,
                request.AffectedCourtIds,
                dateTimeProvider.UtcNow),

            ClosureType.ReducedHours => site.AddReducedHoursClosure(
                request.StartDate,
                request.ModifiedOpeningTime ?? TimeOnly.MinValue,
                request.ModifiedClosingTime ?? TimeOnly.MaxValue,
                request.Reason,
                request.Description,
                request.AffectedCourtIds,
                dateTimeProvider.UtcNow),

            _ => Result.Failure<SiteClosure>(DomainErrors.Site.InvalidClosure)
        };

        if (closureResult.IsFailure)
            return closureResult.PadTimeError;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(closureResult.Value.Id);
    }
}