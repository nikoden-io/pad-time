// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.UpdateSiteSchedule;

/// <summary>
/// Handles <see cref="UpdateSiteScheduleCommand"/> by loading the site with schedules
/// and delegating the update to the domain model.
/// </summary>
public sealed class UpdateSiteScheduleCommandHandler(
    ISiteRepository siteRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<UpdateSiteScheduleCommand, Result>
{
    public async Task<Result> Handle(
        UpdateSiteScheduleCommand request,
        CancellationToken cancellationToken)
    {
        var site = await siteRepository.GetByIdWithSchedulesAndClosuresAsync(request.SiteId, cancellationToken);
        if (site == null)
            return DomainErrors.Site.NotFound;

        var result = site.UpdateSchedule(
            request.ScheduleId,
            request.Name,
            request.ValidFrom,
            request.ValidUntil,
            request.OpeningTime,
            request.ClosingTime,
            request.ApplicableDays,
            request.Priority,
            dateTimeProvider.UtcNow);

        if (result.IsFailure)
            return result.PadTimeError;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}