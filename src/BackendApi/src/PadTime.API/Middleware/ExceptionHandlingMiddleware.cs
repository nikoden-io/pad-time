using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PadTime.Domain.Common;

namespace PadTime.API.Middleware;

/// <summary>
/// Enhanced global exception handling middleware with comprehensive error mapping and logging.
/// Provides consistent error response format across all endpoints with proper HTTP status codes.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly Action<ILogger, string, string, Exception?> LogUnhandledException =
        LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(1, nameof(LogUnhandledException)),
            "Unhandled exception occurred for {Method} {Path}");

    private static readonly Action<ILogger, string, string, string, Exception?> LogValidationException =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Warning,
            new EventId(2, nameof(LogValidationException)),
            "Validation failed for {Method} {Path}: {ValidationErrors}");

    private static readonly Action<ILogger, string, string, string, Exception?> LogDomainException =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Warning,
            new EventId(3, nameof(LogDomainException)),
            "Domain exception occurred for {Method} {Path}: {ErrorCode}");

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next, 
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var request = context.Request;
        var requestInfo = $"{request.Method} {request.Path}";

        var problemDetails = exception switch
        {
            ValidationException validationException => HandleValidationException(validationException, requestInfo),
            UnauthorizedAccessException => HandleUnauthorizedException(),
            ArgumentException argumentException => HandleArgumentException(argumentException),
            InvalidOperationException invalidOperationException => HandleInvalidOperationException(invalidOperationException),
            TimeoutException => HandleTimeoutException(),
            _ => HandleGenericException(exception, requestInfo)
        };

        // Add correlation ID for tracking
        if (context.Items.TryGetValue("CorrelationId", out var correlationId))
        {
            problemDetails.Extensions["correlationId"] = correlationId;
        }

        // Add request ID for debugging
        problemDetails.Extensions["requestId"] = context.TraceIdentifier;

        // Add timestamp
        problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;

        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problemDetails, JsonOptions);
        await context.Response.WriteAsync(json);
    }

    private ValidationProblemDetails HandleValidationException(ValidationException exception, string requestInfo)
    {
        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray(),
                StringComparer.Ordinal);

        var errorSummary = string.Join(", ", errors.Keys);
        LogValidationException(_logger, requestInfo.Split(' ')[0], requestInfo.Split(' ')[1], errorSummary, exception);

        return new ValidationProblemDetails(errors)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "One or more validation errors occurred",
            Status = StatusCodes.Status400BadRequest,
            Detail = "Please check the errors property for details."
        };
    }

    private static ProblemDetails HandleUnauthorizedException()
    {
        return new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
            Title = "Unauthorized",
            Status = StatusCodes.Status401Unauthorized,
            Detail = "Authentication is required to access this resource."
        };
    }

    private static ProblemDetails HandleArgumentException(ArgumentException exception)
    {
        return new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = StatusCodes.Status400BadRequest,
            Detail = exception.Message
        };
    }

    private static ProblemDetails HandleInvalidOperationException(InvalidOperationException exception)
    {
        return new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Invalid Operation",
            Status = StatusCodes.Status400BadRequest,
            Detail = exception.Message
        };
    }

    private static ProblemDetails HandleTimeoutException()
    {
        return new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.4",
            Title = "Request Timeout",
            Status = StatusCodes.Status408RequestTimeout,
            Detail = "The request timed out. Please try again."
        };
    }

    private ProblemDetails HandleGenericException(Exception exception, string requestInfo)
    {
        LogUnhandledException(_logger, requestInfo.Split(' ')[0], requestInfo.Split(' ')[1], exception);

        var detail = _environment.IsDevelopment() 
            ? exception.Message 
            : "An error occurred while processing your request.";

        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = detail
        };

        // Add stack trace in development
        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        return problemDetails;
    }

    private static string GetProblemType(int statusCode)
    {
        return statusCode switch
        {
            400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            401 => "https://tools.ietf.org/html/rfc7235#section-3.1",
            403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            408 => "https://tools.ietf.org/html/rfc7231#section-6.6.4",
            409 => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            422 => "https://tools.ietf.org/html/rfc4918#section-11.2",
            500 => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };
    }

    private static string GetProblemTitle(int statusCode)
    {
        return statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            408 => "Request Timeout",
            409 => "Conflict",
            422 => "Unprocessable Entity",
            500 => "Internal Server Error",
            _ => "Error"
        };
    }
}
