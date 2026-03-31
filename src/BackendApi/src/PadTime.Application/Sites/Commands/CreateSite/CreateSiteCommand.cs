// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.CreateSite;

/// <summary>
/// Command to create a new padel site with address and timezone information.
/// </summary>
/// <param name="Name">Display name of the site.</param>
/// <param name="StreetNumber">Street number of the site address.</param>
/// <param name="Street">Street name of the site address.</param>
/// <param name="Postcode">Postal code of the site address.</param>
/// <param name="City">City of the site address.</param>
/// <param name="Country">Country of the site address.</param>
/// <param name="Timezone">IANA timezone identifier (e.g., "Europe/Brussels").</param>
public sealed record CreateSiteCommand(
    string Name,
    string StreetNumber,
    string Street,
    string Postcode,
    string City,
    string Country,
    string Timezone
    ) : IRequest<Result<Guid>>;