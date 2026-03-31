// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.CreateSiteSchedule;

/// <summary>
/// Handles <see cref="CreateSiteScheduleCommand"/> by loading the site with existing schedules
/// and delegating schedule creation to the domain model.
/// </summary>
public sealed class CreateSiteScheduleCommandHandler(
    ISiteRepository sites,
    IUnitOfWork uow,
    IDateTimeProvider clock)
    : IRequestHandler<CreateSiteScheduleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateSiteScheduleCommand request, CancellationToken cancellationToken)
    {
        var site = await sites.GetByIdWithSchedulesAndClosuresAsync(request.SiteId, cancellationToken);
        if (site is null)
            return DomainErrors.Site.NotFound;

        var scheduleResult = site.AddSchedule(
            name: request.Name,
            validFrom: request.ValidFrom,
            validUntil: request.ValidUntil,
            openingTime: request.OpeningTime,
            closingTime: request.ClosingTime,
            applicableDays: request.ApplicableDays,
            priority: request.Priority,
            utcNow: clock.UtcNow);

        if (scheduleResult.IsFailure)
            return scheduleResult.PadTimeError;

        await uow.SaveChangesAsync(cancellationToken);

        return scheduleResult.Value.Id;
    }
}