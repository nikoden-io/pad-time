// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using Microsoft.AspNetCore.Mvc;
using PadTime.Domain.Common;

namespace PadTime.API.Extensions;

/// <summary>
/// Extension methods for converting domain <see cref="Result"/> and <see cref="Result{T}"/> objects
/// into appropriate ASP.NET Core <see cref="IActionResult"/> responses with RFC 7807 problem details.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a <see cref="Result"/> to an <see cref="IActionResult"/>.
    /// Returns 200 OK on success, or a problem details response on failure.
    /// </summary>
    /// <param name="result">The domain result to convert.</param>
    /// <returns>An appropriate action result.</returns>
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new OkResult();

        return ToProblemDetails(result.PadTimeError);
    }

    /// <summary>
    /// Converts a <see cref="Result{T}"/> to an <see cref="IActionResult"/>.
    /// Returns 200 OK with the value on success, or a problem details response on failure.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The domain result to convert.</param>
    /// <returns>An appropriate action result.</returns>
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        return ToProblemDetails(result.PadTimeError);
    }

    /// <summary>
    /// Converts a <see cref="Result{T}"/> to a 201 Created response on success,
    /// or a problem details response on failure.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The domain result to convert.</param>
    /// <param name="location">The URI of the newly created resource.</param>
    /// <returns>An appropriate action result.</returns>
    public static IActionResult ToCreatedResult<T>(this Result<T> result, string location)
    {
        if (result.IsSuccess)
            return new CreatedResult(location, new { id = result.Value });

        return ToProblemDetails(result.PadTimeError);
    }

    /// <summary>
    /// Creates an RFC 7807 problem details response from a <see cref="PadTimeError"/>.
    /// Maps domain error codes to appropriate HTTP status codes.
    /// </summary>
    /// <param name="padTimeError">The domain error to convert.</param>
    /// <returns>An <see cref="ObjectResult"/> containing the problem details.</returns>
    public static IActionResult ToProblemDetails(PadTimeError padTimeError)
    {
        var statusCode = GetStatusCode(padTimeError.Code);

        var problemDetails = new ProblemDetails
        {
            Type = padTimeError.Code,
            Title = GetTitle(padTimeError.Code),
            Detail = padTimeError.Message,
            Status = statusCode
        };

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
    }

    private static int GetStatusCode(string errorCode)
    {
        return errorCode switch
        {
            // 404 Not Found
            var code when code.Contains("not_found", StringComparison.OrdinalIgnoreCase) ||
                          code.Contains("not.found", StringComparison.OrdinalIgnoreCase)
                => StatusCodes.Status404NotFound,

            // 409 Conflict
            "booking.slot_conflict" => StatusCodes.Status409Conflict,
            "booking.already_participant" => StatusCodes.Status409Conflict,
            "billing.idempotency_conflict" => StatusCodes.Status409Conflict,
            "booking.match_full" => StatusCodes.Status409Conflict,
            "site.schedule_conflict" => StatusCodes.Status409Conflict,
            "site.closure_conflicts_with_bookings" => StatusCodes.Status409Conflict,
            "site.cannot_delete_with_active_bookings" => StatusCodes.Status409Conflict,
            "site.already_deactivated" => StatusCodes.Status409Conflict,
            "site.already_active" => StatusCodes.Status409Conflict,
            "court.duplicate_label" => StatusCodes.Status409Conflict,
            "court.cannot_delete_with_active_bookings" => StatusCodes.Status409Conflict,

            // 403 Forbidden
            "booking.reservation_window_denied" => StatusCodes.Status403Forbidden,
            "booking.site_scope_violation" => StatusCodes.Status403Forbidden,
            "billing.organizer_debt_block" => StatusCodes.Status403Forbidden,
            "booking.not_organizer" => StatusCodes.Status403Forbidden,
            "booking.match_not_public" => StatusCodes.Status403Forbidden,
            "booking.match_locked" => StatusCodes.Status403Forbidden,
            "member.inactive" => StatusCodes.Status403Forbidden,

            // 400 Bad Request
            "booking.invalid_transition" => StatusCodes.Status400BadRequest,
            "billing.invalid_amount" => StatusCodes.Status400BadRequest,
            "member.invalid_matricule" => StatusCodes.Status400BadRequest,

            // Default
            _ => StatusCodes.Status400BadRequest
        };
    }

    private static string GetTitle(string errorCode)
    {
        var statusCode = GetStatusCode(errorCode);
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status409Conflict => "Conflict",
            _ => "Error"
        };
    }
}