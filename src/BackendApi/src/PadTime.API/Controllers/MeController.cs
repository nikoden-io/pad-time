// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PadTime.API.Authorization;
using PadTime.Application.Common.Interfaces;

namespace PadTime.API.Controllers;

/// <summary>
/// Provides the authenticated user's profile information extracted from JWT claims.
/// </summary>
[ApiController]
[Route("api/v1/me")]
[Authorize(Policy = Policies.RequireUser)]
public sealed class MeController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Initializes a new instance of the <see cref="MeController"/> class.
    /// </summary>
    /// <param name="currentUser">Service providing the current authenticated user's claims.</param>
    public MeController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    /// <summary>
    /// Get current user's profile from JWT claims.
    /// </summary>
    /// <returns>The current user's profile information.</returns>
    /// <response code="200">Profile successfully retrieved.</response>
    /// <response code="401">User is not authenticated.</response>
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

/// <summary>
/// Response containing the authenticated user's profile information.
/// </summary>
/// <param name="Subject">The user's unique subject identifier from the identity provider.</param>
/// <param name="Matricule">The user's club matricule number.</param>
/// <param name="Category">The user's membership category (global, site, or free).</param>
/// <param name="Role">The user's authorization role.</param>
/// <param name="SiteId">The user's assigned site identifier, if applicable.</param>
public sealed record MeResponse(
    string Subject,
    string Matricule,
    string Category,
    string Role,
    Guid? SiteId);