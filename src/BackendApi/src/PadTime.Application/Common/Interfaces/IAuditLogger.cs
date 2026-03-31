// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
namespace PadTime.Application.Common.Interfaces;

/// <summary>
/// Interface for audit logging administrative actions.
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Log an administrative action with user context and details.
    /// </summary>
    /// <param name="action">The action performed (e.g., "CreateSite", "DeleteCourt")</param>
    /// <param name="resourceType">The type of resource affected (e.g., "Site", "Court")</param>
    /// <param name="resourceId">The ID of the resource affected</param>
    /// <param name="details">Additional details about the action</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task LogAdministrativeActionAsync(
        string action,
        string resourceType,
        string resourceId,
        object? details = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Log a failed administrative action attempt.
    /// </summary>
    /// <param name="action">The action attempted</param>
    /// <param name="resourceType">The type of resource</param>
    /// <param name="resourceId">The ID of the resource</param>
    /// <param name="reason">The reason for failure</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task LogFailedActionAsync(
        string action,
        string resourceType,
        string resourceId,
        string reason,
        CancellationToken cancellationToken = default);
}