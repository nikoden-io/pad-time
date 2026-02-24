using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.ActivateSite;

public sealed class ActivateSiteCommandHandler(
    ISiteRepository sites,
    IUnitOfWork uow,
    IDateTimeProvider clock)
    : IRequestHandler<ActivateSiteCommand, Result>
{
    public async Task<Result> Handle(ActivateSiteCommand request, CancellationToken cancellationToken)
    {
        var site = await sites.GetByIdAsync(request.SiteId, cancellationToken);
        if (site == null)
            return DomainErrors.Site.NotFound;

        if (site.IsActive)
            return DomainErrors.Site.SiteAlreadyActive;

        site.Activate(clock.UtcNow);

        await sites.UpdateAsync(site, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}