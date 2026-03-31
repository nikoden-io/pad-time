// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.ActivateSite;

/// <summary>
/// Command to activate a previously deactivated site.
/// </summary>
/// <param name="SiteId">Unique identifier of the site to activate.</param>
public sealed record ActivateSiteCommand(Guid SiteId) : IRequest<Result>;