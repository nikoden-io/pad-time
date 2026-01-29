using Microsoft.AspNetCore.Mvc;
using PadTime.Domain.Common;

namespace PadTime.API.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new OkResult();

        return ToProblemDetails(result.PadTimeError);
    }

    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        return ToProblemDetails(result.PadTimeError);
    }

    public static IActionResult ToCreatedResult<T>(this Result<T> result, string location)
    {
        if (result.IsSuccess)
            return new CreatedResult(location, new { id = result.Value });

        return ToProblemDetails(result.PadTimeError);
    }

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
            var code when code.EndsWith("_not_found", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status404NotFound,

            // 409 Conflict
            "booking.slot_conflict" => StatusCodes.Status409Conflict,
            "booking.already_participant" => StatusCodes.Status409Conflict,
            "billing.idempotency_conflict" => StatusCodes.Status409Conflict,
            "booking.match_full" => StatusCodes.Status409Conflict,

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
