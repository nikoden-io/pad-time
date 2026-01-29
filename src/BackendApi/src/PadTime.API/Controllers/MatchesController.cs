using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PadTime.API.Authorization;
using PadTime.API.Extensions;
using PadTime.Application.Booking.Commands.CancelMatch;
using PadTime.Application.Booking.Commands.CreateMatch;
using PadTime.Application.Booking.Commands.JoinMatch;
using PadTime.Application.Booking.Queries.GetMatch;
using PadTime.Domain.Booking;

namespace PadTime.API.Controllers;

[ApiController]
[Route("api/v1/matches")]
[Authorize(Policy = Policies.RequireUser)]
public sealed class MatchesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MatchesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new match (private or public).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateMatchResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateMatch(
        [FromBody] CreateMatchRequest request,
        CancellationToken cancellationToken)
    {
        var type = string.Equals(request.Type, "private", StringComparison.OrdinalIgnoreCase)
            ? PadMatchType.Private
            : PadMatchType.Public;

        var command = new CreateMatchCommand(
            request.SiteId,
            request.CourtId,
            request.StartAt,
            type,
            request.PrivateParticipantsMatricules);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        return Created(
            $"/api/v1/matches/{result.Value}",
            new CreateMatchResponse(result.Value));
    }

    /// <summary>
    /// Get match details.
    /// </summary>
    [HttpGet("{matchId:guid}")]
    [ProducesResponseType(typeof(MatchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMatch(Guid matchId, CancellationToken cancellationToken)
    {
        var query = new GetMatchQuery(matchId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Join a public match (immediate payment required).
    /// </summary>
    [HttpPost("{matchId:guid}/join")]
    [ProducesResponseType(typeof(JoinMatchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> JoinMatch(
        Guid matchId,
        [FromBody] JoinMatchRequest request,
        CancellationToken cancellationToken)
    {
        var command = new JoinMatchCommand(matchId, request.IdempotencyKey);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        return Ok(new JoinMatchResponse(result.Value.PaymentId, result.Value.Status));
    }

    /// <summary>
    /// Cancel a match (organizer before lock, or admin).
    /// </summary>
    [HttpPost("{matchId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelMatch(Guid matchId, CancellationToken cancellationToken)
    {
        var command = new CancelMatchCommand(matchId);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        return NoContent();
    }
}

public sealed record CreateMatchRequest(
    Guid SiteId,
    Guid CourtId,
    DateTime StartAt,
    string Type,
    List<string>? PrivateParticipantsMatricules = null);

public sealed record CreateMatchResponse(Guid MatchId);

public sealed record JoinMatchRequest(string IdempotencyKey);

public sealed record JoinMatchResponse(Guid PaymentId, string Status);
