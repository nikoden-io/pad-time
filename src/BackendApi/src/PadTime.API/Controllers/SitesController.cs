using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PadTime.API.Authorization;
using PadTime.Application.Common.Interfaces.Repositories;

namespace PadTime.API.Controllers;

[ApiController]
[Route("api/v1/sites")]
[Authorize(Policy = Policies.RequireUser)]
public sealed class SitesController : ControllerBase
{
    private readonly ISiteRepository _siteRepository;
    private readonly ICourtRepository _courtRepository;

    public SitesController(ISiteRepository siteRepository, ICourtRepository courtRepository)
    {
        _siteRepository = siteRepository;
        _courtRepository = courtRepository;
    }

    /// <summary>
    /// List all active sites.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SiteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSites(CancellationToken cancellationToken)
    {
        var sites = await _siteRepository.GetAllActiveAsync(cancellationToken);

        var response = sites.Select(s => new SiteResponse(
            s.Id,
            s.Name,
            s.Timezone));

        return Ok(response);
    }

    /// <summary>
    /// Get courts for a site.
    /// </summary>
    [HttpGet("{siteId:guid}/courts")]
    [ProducesResponseType(typeof(IEnumerable<CourtResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourts(Guid siteId, CancellationToken cancellationToken)
    {
        var site = await _siteRepository.GetByIdAsync(siteId, cancellationToken);
        if (site is null)
            return NotFound();

        var courts = await _courtRepository.GetBySiteIdAsync(siteId, cancellationToken);

        var response = courts
            .Where(c => c is not null)
            .Select(c => c!)
            .Where(c => c.IsActive)
            .Select(c => new CourtResponse(c.Id, c.Label, c.IsActive));

        return Ok(response);
    }
}

public sealed record SiteResponse(Guid SiteId, string Name, string Timezone);
public sealed record CourtResponse(Guid CourtId, string Label, bool Active);
