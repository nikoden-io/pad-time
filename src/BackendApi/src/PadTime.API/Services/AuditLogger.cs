using PadTime.Application.Common.Interfaces;
using System.Text.Json;

namespace PadTime.API.Services;

/// <summary>
/// Service for logging administrative actions for audit trails.
/// </summary>
public sealed class AuditLogger : IAuditLogger
{
    private readonly ILogger<AuditLogger> _logger;
    private readonly ICurrentUser _currentUser;

    public AuditLogger(ILogger<AuditLogger> logger, ICurrentUser currentUser)
    {
        _logger = logger;
        _currentUser = currentUser;
    }

    private static readonly Action<ILogger, string, string, string, string, string, string, Exception?> LogAdministrativeAction =
        LoggerMessage.Define<string, string, string, string, string, string>(
            LogLevel.Information,
            new EventId(1, nameof(LogAdministrativeActionAsync)),
            "Administrative Action: {Action} on {ResourceType} {ResourceId} by user {UserId} with role {UserRole}. Details: {Details}");

    private static readonly Action<ILogger, string, string, string, string, string, string, Exception?> LogFailedAction =
        LoggerMessage.Define<string, string, string, string, string, string>(
            LogLevel.Warning,
            new EventId(2, nameof(LogFailedActionAsync)),
            "Failed Administrative Action: {Action} on {ResourceType} {ResourceId} by user {UserId} with role {UserRole}. Reason: {Reason}");

    public Task LogAdministrativeActionAsync(
        string action,
        string resourceType,
        string resourceId,
        object? details = null,
        CancellationToken cancellationToken = default)
    {
        var detailsJson = details != null ? JsonSerializer.Serialize(details) : "None";
        var siteId = _currentUser.SiteId?.ToString();

        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["Action"] = action,
            ["ResourceType"] = resourceType,
            ["ResourceId"] = resourceId,
            ["UserId"] = _currentUser.Subject,
            ["UserRole"] = _currentUser.Role,
            ["UserSiteId"] = siteId,
            ["Success"] = true
        });

        LogAdministrativeAction(
            _logger,
            action,
            resourceType,
            resourceId,
            _currentUser.Subject,
            _currentUser.Role,
            detailsJson,
            null);

        return Task.CompletedTask;
    }

    public Task LogFailedActionAsync(
        string action,
        string resourceType,
        string resourceId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var siteId = _currentUser.SiteId?.ToString();

        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["Action"] = action,
            ["ResourceType"] = resourceType,
            ["ResourceId"] = resourceId,
            ["UserId"] = _currentUser.Subject,
            ["UserRole"] = _currentUser.Role,
            ["UserSiteId"] = siteId,
            ["Success"] = false
        });

        LogFailedAction(
            _logger,
            action,
            resourceType,
            resourceId,
            _currentUser.Subject,
            _currentUser.Role,
            reason,
            null);

        return Task.CompletedTask;
    }
}
