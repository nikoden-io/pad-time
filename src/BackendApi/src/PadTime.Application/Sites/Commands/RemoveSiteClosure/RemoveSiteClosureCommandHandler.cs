// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.RemoveSiteClosure;

/// <summary>
/// Handler for removing a closure (holiday schedule) from a site.
/// </summary>
public sealed class RemoveSiteClosureCommandHandler(
    ISiteRepository siteRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<RemoveSiteClosureCommand, Result>
{
    public async Task<Result> Handle(
        RemoveSiteClosureCommand request,
        CancellationToken cancellationToken)
    {
        var site = await siteRepository.GetByIdWithSchedulesAndClosuresAsync(request.SiteId, cancellationToken);
        if (site == null)
            return DomainErrors.Site.NotFound;

        var result = site.RemoveClosure(request.ClosureId, dateTimeProvider.UtcNow);
        if (result.IsFailure)
            return result.PadTimeError;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}