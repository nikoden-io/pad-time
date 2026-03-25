using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PadTime.API.Authorization;
using PadTime.API.Extensions;
using PadTime.Application.Admin.Queries.GetRevenueAnalytics;
using PadTime.Application.Admin.Queries.GetSiteOverview;

namespace PadTime.API.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Policy = Policies.RequireAdmin)]
public sealed class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns operational alerts for a site: J-1 unprocessed matches,
    /// unpaid participants in upcoming matches, and active organizer debts.
    /// </summary>
    /// <param name="siteId">Identifier of the site.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Overview successfully retrieved.</response>
    /// <response code="403">Admin does not have access to this site.</response>
    /// <response code="404">Site not found.</response>
    [HttpGet("sites/{siteId:guid}/overview")]
    [ProducesResponseType(typeof(SiteOverviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSiteOverview(Guid siteId, CancellationToken cancellationToken)
    {
        var query = new GetSiteOverviewQuery(siteId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Returns aggregated revenue for a site within a date range.
    /// Results are grouped by day and site.
    /// Site admins are automatically restricted to their own site.
    /// </summary>
    /// <param name="siteId">Optional site filter (ignored for site admins, enforced server-side).</param>
    /// <param name="from">Start of the date range (UTC).</param>
    /// <param name="to">End of the date range (UTC).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Revenue analytics successfully retrieved.</response>
    /// <response code="400">Invalid date range.</response>
    [HttpGet("analytics/revenue")]
    [ProducesResponseType(typeof(RevenueAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRevenueAnalytics(
        [FromQuery] Guid? siteId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken cancellationToken)
    {
        if (from >= to)
            return BadRequest("'from' must be before 'to'.");

        var query = new GetRevenueAnalyticsQuery(siteId, from, to);
        var result = await _mediator.Send(query, cancellationToken);

        return result.ToActionResult();
    }
}
