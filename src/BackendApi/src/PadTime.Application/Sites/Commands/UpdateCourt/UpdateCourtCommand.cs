// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.UpdateCourt;

/// <summary>
/// Command to update the label of an existing court within a site.
/// </summary>
/// <param name="SiteId">Identifier of the site containing the court.</param>
/// <param name="CourtId">Identifier of the court to update.</param>
/// <param name="Label">New display label for the court (must be unique within the site).</param>
public sealed record UpdateCourtCommand(
    Guid SiteId,
    Guid CourtId,
    string Label) : IRequest<Result>;