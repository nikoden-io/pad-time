using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.DeactivateSite;

public sealed class DeactivateSiteCommandHandler(
    ISiteRepository sites,
    IUnitOfWork uow,
    IDateTimeProvider clock)
    : IRequestHandler<DeactivateSiteCommand, Result>
{
    public async Task<Result> Handle(DeactivateSiteCommand request, CancellationToken cancellationToken)
    {
        var site = await sites.GetByIdAsync(request.SiteId, cancellationToken);
        if (site == null)
            return DomainErrors.Site.NotFound;

        if (!site.IsActive)
            return DomainErrors.Site.SiteAlreadyDeactivated;

        site.Deactivate(clock.UtcNow);

        await sites.UpdateAsync(site, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}