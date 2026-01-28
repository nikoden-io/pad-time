using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PadTime.API.Authorization;
using PadTime.Application.Booking.Queries.GetAvailability;

namespace PadTime.API.Controllers;

[ApiController]
[Route("api/v1/availability")]
[Authorize(Policy = Policies.RequireUser)]
public sealed class AvailabilityController : ControllerBase
{
    private readonly IMediator _mediator;

    public AvailabilityController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get available time slots for a site on a specific date.
    /// </summary>
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

public sealed record AvailabilityResponse(
    Guid SiteId,
    DateOnly Date,
    IReadOnlyList<SlotResponse> Slots);

public sealed record SlotResponse(
    Guid? CourtId,
    string? CourtLabel,
    DateTime StartAt,
    DateTime EndAt,
    bool Available);
