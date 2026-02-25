using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PadTime.API.Authorization;
using PadTime.API.Extensions;
using PadTime.Application.Booking.Commands.AddParticipant;
using PadTime.Application.Booking.Commands.CancelMatch;
using PadTime.Application.Booking.Commands.CreateMatch;
using PadTime.Application.Booking.Commands.JoinMatch;
using PadTime.Application.Booking.Queries.GetMatch;
using PadTime.Application.Booking.Queries.GetPublicMatches;
using PadTime.Application.Booking.Queries.GetUserMatches;
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
    /// Creates a new match.
    /// Supports both public and private matches.
    /// The current authenticated user becomes the organizer.
    /// For private matches, initial participants can be specified.
    /// </summary>
    /// <param name="request">Match creation parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// Returns the identifier of the created match.
    /// </returns>
    /// <response code="201">Match successfully created.</response>
    /// <response code="400">Invalid request or validation failure.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User is not authorized to create matches.</response>
    /// <response code="409">Match cannot be created due to a business rule conflict.</response>
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
    /// Retrieves the details of a specific match.
    /// Private matches are only visible to participants and administrators.
    /// </summary>
    /// <param name="matchId">Identifier of the match.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// Returns the match details.
    /// </returns>
    /// <response code="200">Match successfully retrieved.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User is not authorized to view this match.</response>
    /// <response code="404">Match was not found.</response>
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
    /// Retrieves matches where the current authenticated user is a participant.
    /// Includes matches organized by the user and matches joined as a participant.
    /// </summary>
    /// <param name="fromUtc">
    /// Optional UTC date filter. When provided, only matches starting on or after this date are returned.
    /// </param>
    /// <param name="page">
    /// Page number for pagination. Starts at 1.
    /// </param>
    /// <param name="pageSize">
    /// Number of matches per page.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// Returns the list of matches for the current user.
    /// </returns>
    /// <response code="200">Matches successfully retrieved.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User is not authorized.</response>
    [HttpGet("user")]
    [ProducesResponseType(typeof(IReadOnlyList<UserMatchDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserMatches(
        [FromQuery] DateTime? fromUtc,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserMatchesQuery(fromUtc, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Retrieves paginated public matches available for joining.
    /// Returns matches with status <c>public</c> or <c>full</c>, ordered by start time.
    /// Defaults to now → now+30 days if no date range is provided.
    /// </summary>
    /// <param name="siteId">Optional site filter.</param>
    /// <param name="fromUtc">Start of search window (UTC). Defaults to now.</param>
    /// <param name="toUtc">End of search window (UTC). Defaults to now + 30 days.</param>
    /// <param name="page">Page number, starting at 1.</param>
    /// <param name="pageSize">Number of results per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Matches successfully retrieved.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet("public")]
    [ProducesResponseType(typeof(IReadOnlyList<PublicMatchDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublicMatches(
        [FromQuery] Guid? siteId = null,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPublicMatchesQuery(siteId, fromUtc, toUtc, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        return result.ToActionResult();
    }

    // Dans MatchesController
    /// <summary>
    /// Adds a participant to a private match by matricule.
    /// Only the organizer can perform this action.
    /// </summary>
    /// <response code="204">Participant successfully added.</response>
    /// <response code="403">Not the organizer.</response>
    /// <response code="404">Match or member not found.</response>
    /// <response code="409">Match is full or participant already registered.</response>
    [HttpPost("{matchId:guid}/participants")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddParticipant(
        Guid matchId,
        [FromBody] AddParticipantRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddParticipantCommand(matchId, request.Matricule);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        return NoContent();
    }

    public sealed record AddParticipantRequest(string Matricule);

    /// <summary>
    /// Joins a public match as a participant.
    /// Immediate payment is required.
    /// Operation is idempotent using the provided idempotency key.
    /// </summary>
    /// <param name="matchId">Identifier of the match to join.</param>
    /// <param name="request">Join match request parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// Returns the payment identifier and current payment status.
    /// </returns>
    /// <response code="200">Successfully joined the match.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User is not authorized to join this match.</response>
    /// <response code="404">Match was not found.</response>
    /// <response code="409">Match cannot be joined due to a business rule conflict.</response>
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
    /// Cancels an existing match.
    /// Only the organizer can cancel before the match is locked.
    /// Administrators may cancel matches according to their scope.
    /// </summary>
    /// <param name="matchId">Identifier of the match to cancel.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// No content is returned on successful cancellation.
    /// </returns>
    /// <response code="204">Match successfully cancelled.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User is not authorized to cancel this match.</response>
    /// <response code="404">Match was not found.</response>
    /// <response code="409">Match cannot be cancelled due to a business rule conflict.</response>
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
