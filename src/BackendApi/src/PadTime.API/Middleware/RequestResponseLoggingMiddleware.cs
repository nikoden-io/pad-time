// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace PadTime.API.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses for audit trails.
/// Provides comprehensive logging of API interactions while respecting sensitive data.
/// </summary>
public sealed class RequestResponseLoggingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private static readonly Action<ILogger, string, string, string, string, int, long, Exception?> LogRequestResponse =
        LoggerMessage.Define<string, string, string, string, int, long>(
            LogLevel.Information,
            new EventId(100, nameof(LogRequestResponse)),
            "HTTP {Method} {Path} from {UserAgent} by {UserId} responded {StatusCode} in {ElapsedMs}ms");

    private static readonly Action<ILogger, string, string, string, Exception?> LogRequestBody =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Debug,
            new EventId(101, nameof(LogRequestBody)),
            "Request {Method} {Path}: {RequestBody}");

    private static readonly Action<ILogger, string, string, int, string, Exception?> LogResponseBody =
        LoggerMessage.Define<string, string, int, string>(
            LogLevel.Debug,
            new EventId(102, nameof(LogResponseBody)),
            "Response {Method} {Path} {StatusCode}: {ResponseBody}");

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestResponseLoggingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    /// <param name="logger">Logger for recording request and response details.</param>
    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Intercepts the HTTP pipeline to log request details, capture the response body,
    /// and record timing information. Skips logging for health, metrics, and Swagger endpoints.
    /// Sensitive fields in JSON payloads are automatically redacted.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Log request details
        var requestBody = await LogRequestAsync(context);
        
        // Capture response
        var originalResponseBodyStream = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            // Log response details
            await LogResponseAsync(context, responseBodyStream, stopwatch.ElapsedMilliseconds);
            
            // Copy response back to original stream
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalResponseBodyStream);
            context.Response.Body = originalResponseBodyStream;
        }
    }

    private async Task<string?> LogRequestAsync(HttpContext context)
    {
        var request = context.Request;
        
        // Skip logging for certain paths
        if (ShouldSkipLogging(request.Path))
            return null;

        string? requestBody = null;
        
        // Log request body for POST/PUT/PATCH requests
        if (ShouldLogRequestBody(request))
        {
            request.EnableBuffering();
            
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            
            // Sanitize sensitive data
            var sanitizedBody = SanitizeRequestBody(requestBody);
            
            LogRequestBody(_logger, request.Method, request.Path, sanitizedBody, null);
        }

        return requestBody;
    }

    private async Task LogResponseAsync(HttpContext context, MemoryStream responseBodyStream, long elapsedMs)
    {
        var request = context.Request;
        var response = context.Response;
        
        // Skip logging for certain paths
        if (ShouldSkipLogging(request.Path))
            return;

        // Get user information
        var userId = context.User?.Identity?.Name ?? "Anonymous";
        var userAgent = request.Headers.UserAgent.ToString();
        
        // Log basic request/response info
        LogRequestResponse(_logger, request.Method, request.Path, userAgent, userId, 
            response.StatusCode, elapsedMs, null);

        // Log response body for errors or debug level
        if (ShouldLogResponseBody(response) && responseBodyStream.Length > 0)
        {
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(responseBodyStream, Encoding.UTF8, leaveOpen: true);
            var responseBody = await reader.ReadToEndAsync();
            
            // Sanitize sensitive data
            var sanitizedBody = SanitizeResponseBody(responseBody);
            
            LogResponseBody(_logger, request.Method, request.Path, response.StatusCode, sanitizedBody, null);
        }
    }

    private static bool ShouldSkipLogging(PathString path)
    {
        var pathValue = path.Value?.ToLowerInvariant();
        
        return pathValue switch
        {
            "/health" => true,
            "/metrics" => true,
            "/swagger" => true,
            var p when p?.StartsWith("/swagger/", StringComparison.OrdinalIgnoreCase) == true => true,
            _ => false
        };
    }

    private static bool ShouldLogRequestBody(HttpRequest request)
    {
        return request.Method is "POST" or "PUT" or "PATCH" &&
               request.ContentType?.Contains("application/json") == true &&
               request.ContentLength is > 0 and < 10_000; // Limit size
    }

    private static bool ShouldLogResponseBody(HttpResponse response)
    {
        return response.StatusCode >= 400 || // Log all error responses
               (response.StatusCode < 300 && response.ContentType?.Contains("application/json") == true);
    }

    private static string SanitizeRequestBody(string requestBody)
    {
        if (string.IsNullOrEmpty(requestBody))
            return requestBody;

        try
        {
            // Parse JSON and remove sensitive fields
            using var document = JsonDocument.Parse(requestBody);
            var sanitized = SanitizeJsonElement(document.RootElement);
            return JsonSerializer.Serialize(sanitized, JsonOptions);
        }
        catch
        {
            // If not valid JSON, return truncated version
            return requestBody.Length > 1000 ? requestBody[..1000] + "..." : requestBody;
        }
    }

    private static string SanitizeResponseBody(string responseBody)
    {
        if (string.IsNullOrEmpty(responseBody))
            return responseBody;

        try
        {
            // Parse JSON and remove sensitive fields
            using var document = JsonDocument.Parse(responseBody);
            var sanitized = SanitizeJsonElement(document.RootElement);
            return JsonSerializer.Serialize(sanitized, JsonOptions);
        }
        catch
        {
            // If not valid JSON, return truncated version
            return responseBody.Length > 1000 ? responseBody[..1000] + "..." : responseBody;
        }
    }

    private static object? SanitizeJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => SanitizeJsonObject(element),
            JsonValueKind.Array => element.EnumerateArray().Select(SanitizeJsonElement).ToArray(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.ToString()
        };
    }

    private static Dictionary<string, object?> SanitizeJsonObject(JsonElement element)
    {
        var result = new Dictionary<string, object?>();
        
        foreach (var property in element.EnumerateObject())
        {
            var propertyName = property.Name.ToLowerInvariant();
            
            // Sanitize sensitive fields
            if (IsSensitiveField(propertyName))
            {
                result[property.Name] = "***REDACTED***";
            }
            else
            {
                result[property.Name] = SanitizeJsonElement(property.Value);
            }
        }
        
        return result;
    }

    private static bool IsSensitiveField(string fieldName)
    {
        return fieldName switch
        {
            "password" => true,
            "token" => true,
            "secret" => true,
            "key" => true,
            "authorization" => true,
            "cookie" => true,
            _ => false
        };
    }
}