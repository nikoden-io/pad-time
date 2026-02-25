using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PadTime.API.Authorization;
using PadTime.API.Extensions;
using PadTime.Application.Billing.Commands.PayMatchParticipation;

namespace PadTime.API.Controllers;

[ApiController]
[Route("api/v1/payments")]
[Authorize(Policy = Policies.RequireUser)]
public sealed class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Pays the participation fee for a private match.
    /// Only the participant themselves can pay their slot.
    /// Operation is idempotent via the provided idempotency key.
    /// </summary>
    /// <response code="200">Payment successful.</response>
    /// <response code="403">Not a participant or already paid.</response>
    /// <response code="404">Match not found.</response>
    /// <response code="409">Idempotency conflict.</response>
    [HttpPost("matches/{matchId:guid}/pay")]
    [ProducesResponseType(typeof(PayMatchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PayMatchParticipation(
        Guid matchId,
        [FromBody] PayMatchRequest request,
        CancellationToken cancellationToken)
    {
        var command = new PayMatchParticipationCommand(matchId, request.IdempotencyKey);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        return Ok(new PayMatchResponse(result.Value.PaymentId, result.Value.Status));
    }
}

public sealed record PayMatchRequest(string IdempotencyKey);
public sealed record PayMatchResponse(Guid PaymentId, string Status);
