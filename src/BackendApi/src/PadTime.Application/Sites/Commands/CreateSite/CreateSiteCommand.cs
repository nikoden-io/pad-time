using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.CreateSite;

public sealed record CreateSiteCommand(
    string Name,
    string StreetNumber,
    string Street,
    string Postcode,
    string City,
    string Country,
    string Timezone
    ) : IRequest<Result<Guid>>;
