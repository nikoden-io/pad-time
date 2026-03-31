// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PadTime.API.Attributes;
using PadTime.API.Authorization;
using PadTime.API.Extensions;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Models;
using PadTime.Application.Sites.Commands.ActivateSite;
using PadTime.Application.Sites.Commands.AddSiteClosure;
using PadTime.Application.Sites.Commands.CreateCourt;
using PadTime.Application.Sites.Commands.CreateSite;
using PadTime.Application.Sites.Commands.CreateSiteSchedule;
using PadTime.Application.Sites.Commands.DeactivateSite;
using PadTime.Application.Sites.Commands.DeleteCourt;
using PadTime.Application.Sites.Commands.DeleteSite;
using PadTime.Application.Sites.Commands.DeleteSiteSchedule;
using PadTime.Application.Sites.Commands.RemoveSiteClosure;
using PadTime.Application.Sites.Commands.UpdateCourt;
using PadTime.Application.Sites.Commands.UpdateSite;
using PadTime.Application.Sites.Commands.UpdateSiteSchedule;
using PadTime.Application.Sites.Queries.GetCourtById;
using PadTime.Application.Sites.Queries.GetCourts;
using PadTime.Application.Sites.Queries.GetSiteById;
using PadTime.Application.Sites.Queries.GetSites;
using PadTime.Application.Sites.Queries.GetSiteSchedule;
using PadTime.Application.Sites.Queries.GetSiteStatistics;
using PadTime.Domain.Common;
using PadTime.Domain.Site;

namespace PadTime.API.Controllers;

/// <summary>
///     Manages site-related operations.
/// </summary>
[ApiController]
[Route("api/v1/sites")]
[Authorize(Policy = Policies.RequireUser)]
public sealed class SitesController(IMediator mediator, IAuditLogger auditLogger) : ControllerBase
{
    /// <summary>
    ///     Create a new site.
    /// </summary>
    [HttpPost]
    [ValidateModel]
    [Authorize(Policy = Policies.RequireGlobalAdmin)]
    [ProducesResponseType(typeof(CreateSiteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSite(
        [FromBody] CreateSiteRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateSiteCommand(request.Name, request.StreetNumber, request.Street, request.Postcode,
            request.City, request.Country, request.Timezone);
        Result<Guid> result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            await auditLogger.LogFailedActionAsync(
                "CreateSite",
                "Site",
                "N/A",
                result.PadTimeError.Message,
                cancellationToken);
            return result.ToActionResult();
        }

        await auditLogger.LogAdministrativeActionAsync(
            "CreateSite",
            "Site",
            result.Value.ToString(),
            new { request.Name, request.City, request.Country },
            cancellationToken);

        return Created(
            $"/api/v1/sites/{result.Value}",
            new CreateSiteResponse(result.Value));
    }

    /// <summary>
    ///     Get sites with pagination, search, and filtering support.
    /// </summary>
    [HttpGet]
    [ValidateModel]
    [Authorize(Policy = Policies.RequireUser)]
    [ProducesResponseType(typeof(PagedResult<SiteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSites(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? city = null,
        [FromQuery] string? country = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSitesQuery(page, pageSize, searchTerm, isActive, city, country);
        PagedResult<SiteDto> result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    ///     Get detailed site information by ID.
    /// </summary>
    [HttpGet("{siteId:guid}")]
    [Authorize(Policy = Policies.RequireSiteAccess)]
    [ProducesResponseType(typeof(SiteDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSiteById(Guid siteId, CancellationToken cancellationToken)
    {
        var query = new GetSiteByIdQuery(siteId);
        Result<SiteDetailDto> result = await mediator.Send(query, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        return Ok(result.Value);
    }

    /// <summary>
    ///     Get site statistics for dashboard.
    /// </summary>
    [HttpGet("{siteId:guid}/statistics")]
    [Authorize(Policy = Policies.RequireSiteAccess)]
    [ProducesResponseType(typeof(SiteStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSiteStatistics(Guid siteId, CancellationToken cancellationToken)
    {
        var query = new GetSiteStatisticsQuery(siteId);
        Result<SiteStatisticsDto> result = await mediator.Send(query, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        return Ok(result.Value);
    }

    /// <summary>
    ///     Update an existing site.
    /// </summary>
    [HttpPut("{siteId:guid}")]
    [ValidateModel]
    [Authorize(Policy = Policies.RequireSiteManagement)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSite(
        Guid siteId,
        [FromBody] UpdateSiteRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSiteCommand(
            siteId,
            request.Name,
            request.StreetNumber,
            request.Street,
            request.Postcode,
            request.City,
            request.Country,
            request.Timezone);

        Result result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            await auditLogger.LogFailedActionAsync(
                "UpdateSite",
                "Site",
                siteId.ToString(),
                result.PadTimeError.Message,
                cancellationToken);
            return result.ToActionResult();
        }

        await auditLogger.LogAdministrativeActionAsync(
            "UpdateSite",
            "Site",
            siteId.ToString(),
            new { request.Name, request.City, request.Country },
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    ///     Delete a site with safety checks.
    /// </summary>
    [HttpDelete("{siteId:guid}")]
    [Authorize(Policy = Policies.RequireGlobalAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteSite(Guid siteId, CancellationToken cancellationToken)
    {
        var command = new DeleteSiteCommand(siteId);
        Result result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            await auditLogger.LogFailedActionAsync(
                "DeleteSite",
                "Site",
                siteId.ToString(),
                result.PadTimeError.Message,
                cancellationToken);
            return result.ToActionResult();
        }

        await auditLogger.LogAdministrativeActionAsync(
            "DeleteSite",
            "Site",
            siteId.ToString(),
            null,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    ///     Deactivate a site when deletion is not possible due to existing bookings.
    /// </summary>
    [HttpPost("{siteId:guid}/deactivate")]
    [Authorize(Policy = Policies.RequireSiteManagement)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateSite(Guid siteId, CancellationToken cancellationToken)
    {
        var command = new DeactivateSiteCommand(siteId);
        Result result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            await auditLogger.LogFailedActionAsync(
                "DeactivateSite",
                "Site",
                siteId.ToString(),
                result.PadTimeError.Message,
                cancellationToken);
            return result.ToActionResult();
        }

        await auditLogger.LogAdministrativeActionAsync(
            "DeactivateSite",
            "Site",
            siteId.ToString(),
            null,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    ///     Activate a previously deactivated site.
    /// </summary>
    [HttpPost("{siteId:guid}/activate")]
    [Authorize(Policy = Policies.RequireSiteManagement)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateSite(Guid siteId, CancellationToken cancellationToken)
    {
        var command = new ActivateSiteCommand(siteId);
        Result result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            await auditLogger.LogFailedActionAsync(
                "ActivateSite",
                "Site",
                siteId.ToString(),
                result.PadTimeError.Message,
                cancellationToken);
            return result.ToActionResult();
        }

        await auditLogger.LogAdministrativeActionAsync(
            "ActivateSite",
            "Site",
            siteId.ToString(),
            null,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    ///     Get all courts for a specific site.
    /// </summary>
    [HttpGet("{siteId:guid}/courts")]
    [Authorize(Policy = Policies.RequireSiteAccess)]
    [ProducesResponseType(typeof(IEnumerable<CourtDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourts(Guid siteId, CancellationToken cancellationToken)
    {
        var query = new GetCourtsQuery(siteId);
        Result<List<CourtDto>> result = await mediator.Send(query, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        return Ok(result.Value);
    }

    /// <summary>
    ///     Get a specific court by ID.
    /// </summary>
    [HttpGet("{siteId:guid}/courts/{courtId:guid}")]
    [Authorize(Policy = Policies.RequireSiteAccess)]
    [ProducesResponseType(typeof(CourtByIdDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourtById(Guid siteId, Guid courtId, CancellationToken cancellationToken)
    {
        var query = new GetCourtByIdQuery(courtId);
        Result<CourtByIdDto> result = await mediator.Send(query, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        return Ok(result.Value);
    }

    /// <summary>
    ///     Create a new court for a site.
    /// </summary>
    [HttpPost("{siteId:guid}/courts")]
    [ValidateModel]
    [Authorize(Policy = Policies.RequireSiteManagement)]
    [ProducesResponseType(typeof(CreateCourtResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateCourt(
        Guid siteId,
        [FromBody] CreateCourtRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCourtCommand(siteId, request.Label);
        Result<Guid> result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            await auditLogger.LogFailedActionAsync(
                "CreateCourt",
                "Court",
                "N/A",
                result.PadTimeError.Message,
                cancellationToken);
            return result.ToActionResult();
        }

        await auditLogger.LogAdministrativeActionAsync(
            "CreateCourt",
            "Court",
            result.Value.ToString(),
            new { SiteId = siteId, request.Label },
            cancellationToken);

        return Created(
            $"/api/v1/sites/{siteId}/courts/{result.Value}",
            new CreateCourtResponse(result.Value));
    }

    /// <summary>
    ///     Update an existing court.
    /// </summary>
    [HttpPut("{siteId:guid}/courts/{courtId:guid}")]
    [ValidateModel]
    [Authorize(Policy = Policies.RequireSiteManagement)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCourt(
        Guid siteId,
        Guid courtId,
        [FromBody] UpdateCourtRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCourtCommand(siteId, courtId, request.Label);
        Result result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            await auditLogger.LogFailedActionAsync(
                "UpdateCourt",
                "Court",
                courtId.ToString(),
                result.PadTimeError.Message,
                cancellationToken);
            return result.ToActionResult();
        }

        await auditLogger.LogAdministrativeActionAsync(
            "UpdateCourt",
            "Court",
            courtId.ToString(),
            new { SiteId = siteId, request.Label },
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    ///     Delete a court with safety checks.
    /// </summary>
    [HttpDelete("{siteId:guid}/courts/{courtId:guid}")]
    [Authorize(Policy = Policies.RequireSiteManagement)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCourt(Guid siteId, Guid courtId, CancellationToken cancellationToken)
    {
        var command = new DeleteCourtCommand(siteId, courtId);
        Result result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            await auditLogger.LogFailedActionAsync(
                "DeleteCourt",
                "Court",
                courtId.ToString(),
                result.PadTimeError.Message,
                cancellationToken);
            return result.ToActionResult();
        }

        await auditLogger.LogAdministrativeActionAsync(
            "DeleteCourt",
            "Court",
            courtId.ToString(),
            new { SiteId = siteId },
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    ///     Get site schedule including regular schedules and holiday closures.
    /// </summary>
    [HttpGet("{siteId:guid}/schedules")]
    [Authorize(Policy = Policies.RequireSiteAccess)]
    [ProducesResponseType(typeof(SiteScheduleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSiteSchedule(Guid siteId, CancellationToken cancellationToken)
    {
        var query = new GetSiteScheduleQuery(siteId);
        Result<SiteScheduleDetailDto> result = await mediator.Send(query, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        return Ok(result.Value);
    }

    /// <summary>
    ///     Create a new schedule for a site.
    /// </summary>
    [HttpPost("{siteId:guid}/schedules")]
    [ValidateModel]
    [Authorize(Policy = Policies.RequireSiteManagement)]
    [ProducesResponseType(typeof(CreateSiteScheduleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateSiteSchedule(
        Guid siteId,
        [FromBody] CreateSiteScheduleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateSiteScheduleCommand(
            siteId,
            request.Name,
            request.ValidFrom,
            request.ValidUntil,
            request.OpeningTime,
            request.ClosingTime,
            request.ApplicableDays,
            request.Priority);

        Result<Guid> result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            await auditLogger.LogFailedActionAsync(
                "CreateSiteSchedule",
                "SiteSchedule",
                "N/A",
                result.PadTimeError.Message,
                cancellationToken);
            return result.ToActionResult();
        }

        await auditLogger.LogAdministrativeActionAsync(
            "CreateSiteSchedule",
            "SiteSchedule",
            result.Value.ToString(),
            new { SiteId = siteId, request.Name, request.ValidFrom, request.ValidUntil },
            cancellationToken);

        return Created(
            $"/api/v1/sites/{siteId}/schedules/{result.Value}",
            new CreateSiteScheduleResponse(result.Value));
    }

    /// <summary>
    ///     Update site schedule.
    /// </summary>
    [HttpPut("{siteId:guid}/schedules/{scheduleId:guid}")]
    [ValidateModel]
    [Authorize(Policy = Policies.RequireSiteManagement)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSiteSchedule(
        Guid siteId,
        Guid scheduleId,
        [FromBody] UpdateSiteScheduleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSiteScheduleCommand(
            siteId,
            scheduleId,
            request.Name,
            request.ValidFrom,
            request.ValidUntil,
            request.OpeningTime,
            request.ClosingTime,
            request.ApplicableDays,
            request.Priority);

        Result result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            await auditLogger.LogFailedActionAsync(
                "UpdateSiteSchedule",
                "SiteSchedule",
                scheduleId.ToString(),
                result.PadTimeError.Message,
                cancellationToken);
            return result.ToActionResult();
        }

        await auditLogger.LogAdministrativeActionAsync(
            "UpdateSiteSchedule",
            "SiteSchedule",
            scheduleId.ToString(),
            new { SiteId = siteId, request.Name, request.ValidFrom, request.ValidUntil },
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    ///     Delete a site schedule.
    /// </summary>
    [HttpDelete("{siteId:guid}/schedules/{scheduleId:guid}")]
    [Authorize(Policy = Policies.RequireSiteManagement)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSiteSchedule(
        Guid siteId,
        Guid scheduleId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteSiteScheduleCommand(siteId, scheduleId);
        Result result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            await auditLogger.LogFailedActionAsync(
                "DeleteSiteSchedule",
                "SiteSchedule",
                scheduleId.ToString(),
                result.PadTimeError.Message,
                cancellationToken);
            return result.ToActionResult();
        }

        await auditLogger.LogAdministrativeActionAsync(
            "DeleteSiteSchedule",
            "SiteSchedule",
            scheduleId.ToString(),
            new { SiteId = siteId },
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    ///     Add a closure to a site.
    /// </summary>
    [HttpPost("{siteId:guid}/closures")]
    [ValidateModel]
    [Authorize(Policy = Policies.RequireSiteManagement)]
    [ProducesResponseType(typeof(AddSiteClosureResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddSiteClosure(
        Guid siteId,
        [FromBody] AddSiteClosureRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddSiteClosureCommand(
            siteId,
            request.Type,
            request.Reason,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.ModifiedOpeningTime,
            request.ModifiedClosingTime,
            request.AffectedCourtIds);

        Result<Guid> result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            await auditLogger.LogFailedActionAsync(
                "AddSiteClosure",
                "SiteClosure",
                "N/A",
                result.PadTimeError.Message,
                cancellationToken);
            return result.ToActionResult();
        }

        await auditLogger.LogAdministrativeActionAsync(
            "AddSiteClosure",
            "SiteClosure",
            result.Value.ToString(),
            new { SiteId = siteId, request.Type, request.Reason, request.StartDate, request.EndDate },
            cancellationToken);

        return Created(
            $"/api/v1/sites/{siteId}/closures/{result.Value}",
            new AddSiteClosureResponse(result.Value));
    }

    /// <summary>
    ///     Remove a holiday closure from a site schedule.
    /// </summary>
    [HttpDelete("{siteId:guid}/closures/{closureId:guid}")]
    [Authorize(Policy = Policies.RequireSiteManagement)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveSiteClosure(
        Guid siteId,
        Guid closureId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveSiteClosureCommand(siteId, closureId);
        Result result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            await auditLogger.LogFailedActionAsync(
                "RemoveSiteClosure",
                "SiteClosure",
                closureId.ToString(),
                result.PadTimeError.Message,
                cancellationToken);
            return result.ToActionResult();
        }

        await auditLogger.LogAdministrativeActionAsync(
            "RemoveSiteClosure",
            "SiteClosure",
            closureId.ToString(),
            new { SiteId = siteId },
            cancellationToken);

        return NoContent();
    }
}

/// <summary>Request body for creating a new court.</summary>
/// <param name="Label">Display label for the court (e.g., "Court 1").</param>
public sealed record CreateCourtRequest(string Label);

/// <summary>Response returned after successful court creation.</summary>
/// <param name="CourtId">Identifier of the newly created court.</param>
public sealed record CreateCourtResponse(Guid CourtId);

/// <summary>Request body for creating a new site.</summary>
/// <param name="Name">Name of the site.</param>
/// <param name="StreetNumber">Street number of the site address.</param>
/// <param name="Street">Street name of the site address.</param>
/// <param name="Postcode">Postal code of the site address.</param>
/// <param name="City">City of the site address.</param>
/// <param name="Country">Country of the site address.</param>
/// <param name="Timezone">IANA timezone identifier (e.g., "Europe/Brussels").</param>
public sealed record CreateSiteRequest(
    string Name,
    string StreetNumber,
    string Street,
    string Postcode,
    string City,
    string Country,
    string Timezone
);

/// <summary>Response returned after successful site creation.</summary>
/// <param name="SiteId">Identifier of the newly created site.</param>
public sealed record CreateSiteResponse(Guid SiteId);

/// <summary>Request body for updating an existing site.</summary>
/// <param name="Name">Updated name of the site.</param>
/// <param name="StreetNumber">Updated street number.</param>
/// <param name="Street">Updated street name.</param>
/// <param name="Postcode">Updated postal code.</param>
/// <param name="City">Updated city.</param>
/// <param name="Country">Updated country.</param>
/// <param name="Timezone">Updated IANA timezone identifier.</param>
public sealed record UpdateSiteRequest(
    string Name,
    string StreetNumber,
    string Street,
    string Postcode,
    string City,
    string Country,
    string Timezone
);

/// <summary>Request body for updating a court.</summary>
/// <param name="Label">Updated display label for the court.</param>
public sealed record UpdateCourtRequest(string Label);

/// <summary>Request body for creating a site schedule.</summary>
/// <param name="Name">Display name of the schedule.</param>
/// <param name="ValidFrom">Start date of the schedule validity period.</param>
/// <param name="ValidUntil">Optional end date of the schedule validity period.</param>
/// <param name="OpeningTime">Daily opening time.</param>
/// <param name="ClosingTime">Daily closing time.</param>
/// <param name="ApplicableDays">Days of the week this schedule applies to. Null means all days.</param>
/// <param name="Priority">Priority for schedule resolution when multiple schedules overlap.</param>
public sealed record CreateSiteScheduleRequest(
    string Name,
    DateOnly ValidFrom,
    DateOnly? ValidUntil,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime,
    DayOfWeek[]? ApplicableDays,
    int Priority
);

/// <summary>Response returned after successful schedule creation.</summary>
/// <param name="ScheduleId">Identifier of the newly created schedule.</param>
public sealed record CreateSiteScheduleResponse(Guid ScheduleId);

/// <summary>Request body for updating a site schedule.</summary>
/// <param name="Name">Updated display name of the schedule.</param>
/// <param name="ValidFrom">Updated start date of the schedule validity period.</param>
/// <param name="ValidUntil">Updated optional end date of the schedule validity period.</param>
/// <param name="OpeningTime">Updated daily opening time.</param>
/// <param name="ClosingTime">Updated daily closing time.</param>
/// <param name="ApplicableDays">Updated applicable days of the week. Null means all days.</param>
/// <param name="Priority">Updated priority for schedule resolution.</param>
public sealed record UpdateSiteScheduleRequest(
    string Name,
    DateOnly ValidFrom,
    DateOnly? ValidUntil,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime,
    DayOfWeek[]? ApplicableDays,
    int Priority
);

/// <summary>Request body for adding a site closure.</summary>
/// <param name="Type">The type of closure (full day, modified hours, etc.).</param>
/// <param name="Reason">The reason for the closure (public holiday, maintenance, etc.).</param>
/// <param name="Description">Optional description providing additional context.</param>
/// <param name="StartDate">Start date of the closure period.</param>
/// <param name="EndDate">Optional end date of the closure period.</param>
/// <param name="ModifiedOpeningTime">Modified opening time during the closure, if applicable.</param>
/// <param name="ModifiedClosingTime">Modified closing time during the closure, if applicable.</param>
/// <param name="AffectedCourtIds">Optional list of specific court identifiers affected by the closure.</param>
public sealed record AddSiteClosureRequest(
    ClosureType Type,
    ClosureReason Reason,
    string? Description,
    DateOnly StartDate,
    DateOnly? EndDate,
    TimeOnly? ModifiedOpeningTime,
    TimeOnly? ModifiedClosingTime,
    Guid[]? AffectedCourtIds
);

/// <summary>Response returned after successful closure creation.</summary>
/// <param name="ClosureId">Identifier of the newly created closure.</param>
public sealed record AddSiteClosureResponse(Guid ClosureId);