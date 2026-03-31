// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.DeactivateSite;

/// <summary>
/// Command to deactivate an active site, preventing new bookings.
/// </summary>
/// <param name="SiteId">Unique identifier of the site to deactivate.</param>
public sealed record DeactivateSiteCommand(Guid SiteId) : IRequest<Result>;