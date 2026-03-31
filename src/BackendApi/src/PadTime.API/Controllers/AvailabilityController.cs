// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PadTime.API.Authorization;
using PadTime.Application.Booking.Queries.GetAvailability;

namespace PadTime.API.Controllers;

/// <summary>
/// Provides endpoints for querying court availability at a given site and date.
/// </summary>
[ApiController]
[Route("api/v1/availability")]
[Authorize(Policy = Policies.RequireUser)]
public sealed class AvailabilityController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvailabilityController"/> class.
    /// </summary>
    /// <param name="mediator">MediatR mediator for dispatching queries.</param>
    public AvailabilityController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get available time slots for a site on a specific date.
    /// </summary>
    /// <param name="siteId">Identifier of the site to query availability for.</param>
    /// <param name="date">The date to check availability.</param>
    /// <param name="courtId">Optional court filter. When provided, only slots for that court are returned.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of time slots with their availability status.</returns>
    /// <response code="200">Availability slots successfully retrieved.</response>
    [HttpGet]
    [ProducesResponseType(typeof(AvailabilityResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailability(
        [FromQuery] Guid siteId,
        [FromQuery] DateOnly date,
        [FromQuery] Guid? courtId,
        CancellationToken cancellationToken)
    {
        var query = new GetAvailabilityQuery(siteId, date, courtId);
        var result = await _mediator.Send(query, cancellationToken);

        var response = new AvailabilityResponse(
            result.SiteId,
            result.Date,
            result.Slots.Select(s => new SlotResponse(
                s.CourtId,
                s.CourtLabel,
                s.StartAtUtc,
                s.EndAtUtc,
                s.Available)).ToList());

        return Ok(response);
    }
}

/// <summary>
/// Response containing availability slots for a site on a given date.
/// </summary>
/// <param name="SiteId">Identifier of the queried site.</param>
/// <param name="Date">The date for which availability was checked.</param>
/// <param name="Slots">The list of time slots with availability information.</param>
public sealed record AvailabilityResponse(
    Guid SiteId,
    DateOnly Date,
    IReadOnlyList<SlotResponse> Slots);

/// <summary>
/// Represents a single time slot on a court with its availability status.
/// </summary>
/// <param name="CourtId">Identifier of the court, if applicable.</param>
/// <param name="CourtLabel">Display label of the court.</param>
/// <param name="StartAt">Start time of the slot (UTC).</param>
/// <param name="EndAt">End time of the slot (UTC).</param>
/// <param name="Available">Whether the slot is available for booking.</param>
public sealed record SlotResponse(
    Guid? CourtId,
    string? CourtLabel,
    DateTime StartAt,
    DateTime EndAt,
    bool Available);