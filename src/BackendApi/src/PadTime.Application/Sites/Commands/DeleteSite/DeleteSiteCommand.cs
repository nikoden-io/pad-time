// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.DeleteSite;

/// <summary>
/// Command to permanently delete a site. Fails if the site has active or future bookings.
/// </summary>
/// <param name="SiteId">Unique identifier of the site to delete.</param>
public sealed record DeleteSiteCommand(Guid SiteId) : IRequest<Result>;