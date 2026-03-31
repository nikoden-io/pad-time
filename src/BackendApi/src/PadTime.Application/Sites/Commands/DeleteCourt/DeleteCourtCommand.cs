// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Commands.DeleteCourt;

/// <summary>
/// Command to delete a court from a site. Fails if the court has active or future bookings.
/// </summary>
/// <param name="SiteId">Identifier of the site containing the court.</param>
/// <param name="CourtId">Identifier of the court to delete.</param>
public sealed record DeleteCourtCommand(
    Guid SiteId,
    Guid CourtId) : IRequest<Result>;