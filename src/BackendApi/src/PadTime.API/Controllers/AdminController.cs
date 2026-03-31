// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PadTime.API.Authorization;
using PadTime.API.Extensions;
using PadTime.Application.Admin.Commands.ToggleMemberStatus;
using PadTime.Application.Admin.Queries.GetMemberDetail;
using PadTime.Application.Admin.Queries.GetMembers;
using PadTime.Application.Admin.Queries.GetRevenueAnalytics;
using PadTime.Application.Admin.Queries.GetSiteOverview;
using PadTime.Domain.Members;

namespace PadTime.API.Controllers;

/// <summary>
/// Provides administrative endpoints for site oversight, revenue analytics, and member management.
/// All actions require an admin-level authorization policy.
/// </summary>
[ApiController]
[Route("api/v1/admin")]
[Authorize(Policy = Policies.RequireAdmin)]
public sealed class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminController"/> class.
    /// </summary>
    /// <param name="mediator">MediatR mediator for dispatching queries and commands.</param>
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

    /// <summary>
    /// Returns a paginated list of members with optional filters.
    /// </summary>
    [HttpGet("members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMembers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? category = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        MemberCategory? parsedCategory = null;
        if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<MemberCategory>(category, true, out var cat))
            parsedCategory = cat;

        var query = new GetMembersQuery(page, pageSize, parsedCategory, isActive, search);
        var result = await _mediator.Send(query, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Returns detailed information about a specific member.
    /// </summary>
    [HttpGet("members/{memberId:guid}")]
    [ProducesResponseType(typeof(MemberDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMemberDetail(Guid memberId, CancellationToken cancellationToken)
    {
        var query = new GetMemberDetailQuery(memberId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Activates a member account.
    /// </summary>
    [HttpPost("members/{memberId:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateMember(Guid memberId, CancellationToken cancellationToken)
    {
        var command = new ToggleMemberStatusCommand(memberId, IsActive: true);
        var result = await _mediator.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Deactivates a member account.
    /// </summary>
    [HttpPost("members/{memberId:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateMember(Guid memberId, CancellationToken cancellationToken)
    {
        var command = new ToggleMemberStatusCommand(memberId, IsActive: false);
        var result = await _mediator.Send(command, cancellationToken);

        return result.ToActionResult();
    }
}