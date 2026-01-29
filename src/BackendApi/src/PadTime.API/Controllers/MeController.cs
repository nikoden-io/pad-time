using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PadTime.API.Authorization;
using PadTime.Application.Common.Interfaces;

namespace PadTime.API.Controllers;

[ApiController]
[Route("api/v1/me")]
[Authorize(Policy = Policies.RequireUser)]
public sealed class MeController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    public MeController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    /// <summary>
    /// Get current user's profile from JWT claims.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(MeResponse), StatusCodes.Status200OK)]
    public IActionResult GetMe()
    {
        var response = new MeResponse(
            Subject: _currentUser.Subject,
            Matricule: _currentUser.Matricule,
            Category: _currentUser.Category.ToString().ToLowerInvariant(),
            Role: _currentUser.Role,
            SiteId: _currentUser.SiteId);

        return Ok(response);
    }
}

public sealed record MeResponse(
    string Subject,
    string Matricule,
    string Category,
    string Role,
    Guid? SiteId);
