// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.UpdateSite;

/// <summary>
/// Command to update the name, address, and timezone of an existing site.
/// </summary>
/// <param name="SiteId">Identifier of the site to update.</param>
/// <param name="Name">Updated site name.</param>
/// <param name="StreetNumber">Updated street number.</param>
/// <param name="Street">Updated street name.</param>
/// <param name="Postcode">Updated postal code.</param>
/// <param name="City">Updated city.</param>
/// <param name="Country">Updated country.</param>
/// <param name="Timezone">Updated IANA timezone identifier.</param>
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