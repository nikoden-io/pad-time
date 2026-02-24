using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;
using PadTime.Domain.Site;

namespace PadTime.Application.Sites.Commands.CreateSite;

public sealed class CreateSiteCommandHandler(
    ISiteRepository sites,
    IUnitOfWork uow,
    IDateTimeProvider clock)
    : IRequestHandler<CreateSiteCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateSiteCommand request, CancellationToken cancellationToken)
    {
        var site = Site.Create(
            name: request.Name,
            streetNumber: request.StreetNumber,
            street: request.Street,
            postcode: request.Postcode,
            city: request.City,
            country: request.Country,
            timezone: request.Timezone,
            utcNow: clock.UtcNow);

        await sites.AddAsync(site, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return Result.Success(site.Id);
    }
}
