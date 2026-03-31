// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.CreateCourt;

/// <summary>
/// Command to create a new court within a site.
/// </summary>
/// <param name="SiteId">Identifier of the site to add the court to.</param>
/// <param name="Label">Display label for the court (must be unique within the site).</param>
public sealed record CreateCourtCommand(
    Guid SiteId,
    string Label) : IRequest<Result<Guid>>;