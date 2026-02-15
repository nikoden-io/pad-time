using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.UpdateSite;

public sealed record UpdateSiteCommand(
    Guid SiteId,
    string Name,
    string StreetNumber,
    string Street,
    string Postcode,
    string City,
    string Country,
    string Timezone
    ) : IRequest<Result>;