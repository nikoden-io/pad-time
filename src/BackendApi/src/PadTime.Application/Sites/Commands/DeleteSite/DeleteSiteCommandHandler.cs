// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.DeleteSite;

/// <summary>
/// Handles <see cref="DeleteSiteCommand"/> by verifying the site exists and has no active bookings
/// before permanently deleting it.
/// </summary>
public sealed class DeleteSiteCommandHandler(
    ISiteRepository sites,
    IUnitOfWork uow)
    : IRequestHandler<DeleteSiteCommand, Result>
{
    public async Task<Result> Handle(DeleteSiteCommand request, CancellationToken cancellationToken)
    {
        var site = await sites.GetByIdAsync(request.SiteId, cancellationToken);
        if (site == null)
            return DomainErrors.Site.NotFound;

        // Check for active or future bookings
        var hasActiveBookings = await sites.HasActiveBookingsAsync(request.SiteId, cancellationToken);
        if (hasActiveBookings)
            return DomainErrors.Site.CannotDeleteSiteWithActiveBookings;

        await sites.DeleteAsync(site, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}