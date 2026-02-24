using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.UpdateSite;

public sealed class UpdateSiteCommandHandler(
    ISiteRepository sites,
    IUnitOfWork uow,
    IDateTimeProvider clock)
    : IRequestHandler<UpdateSiteCommand, Result>
{
    public async Task<Result> Handle(UpdateSiteCommand request, CancellationToken cancellationToken)
    {
        var site = await sites.GetByIdAsync(request.SiteId, cancellationToken);
        if (site == null)
            return DomainErrors.Site.NotFound;

        site.UpdateInformation(
            name: request.Name,
            streetNumber: request.StreetNumber,
            street: request.Street,
            postcode: request.Postcode,
            city: request.City,
            country: request.Country,
            timezone: request.Timezone,
            utcNow: clock.UtcNow);

        await sites.UpdateAsync(site, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}