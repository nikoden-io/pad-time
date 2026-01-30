using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PadTime.API.Authorization;
using PadTime.Application.Sites.Queries.GetSites;

namespace PadTime.API.Controllers;

/// <summary>
/// Manages site-related operations.
/// </summary>
[ApiController]
[Route("api/v1/sites")]
[Authorize(Policy = Policies.RequireUser)]
public sealed class SitesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get all active sites.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<SiteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSites(CancellationToken cancellationToken)
    {
        var query = new GetSitesQuery();
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
