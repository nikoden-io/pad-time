// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
namespace PadTime.Application.Common.Interfaces;

/// <summary>
/// Generic AI completion service for structured JSON responses.
/// Returns null on failure (non-critical feature).
/// </summary>
public interface IAiCompletionService
{
    /// <summary>
    /// Sends a prompt to the AI and returns the raw JSON response text.
    /// Returns null on any failure.
    /// </summary>
    Task<string?> CompleteJsonAsync(string prompt, CancellationToken cancellationToken = default);
}
