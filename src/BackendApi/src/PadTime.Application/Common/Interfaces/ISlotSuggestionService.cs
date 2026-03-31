// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
namespace PadTime.Application.Common.Interfaces;

/// <summary>
/// Abstraction for AI-powered slot suggestion generation.
/// The implementation handles the external LLM call and JSON parsing.
/// </summary>
public interface ISlotSuggestionService
{
    /// <summary>
    /// Generates slot suggestions based on the provided context.
    /// Returns an empty list on failure (non-critical feature).
    /// </summary>
    Task<IReadOnlyList<RawSlotSuggestion>> GetSuggestionsAsync(
        SlotSuggestionContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Aggregated context passed to the suggestion service for prompt building.
/// </summary>
public sealed record SlotSuggestionContext(
    MemberPlayPattern PlayPattern,
    IReadOnlyList<AvailableSlotGroup> AvailableSlots,
    IReadOnlyList<CourtUtilizationInfo> CourtUtilization);

/// <summary>
/// Summarised play patterns extracted from a member's match history.
/// </summary>
public sealed record MemberPlayPattern(
    IReadOnlyDictionary<DayOfWeek, int> DayOfWeekFrequency,
    IReadOnlyList<string> PreferredTimeWindows,
    IReadOnlyList<Guid> PreferredSiteIds,
    IReadOnlyList<Guid> PreferredCourtIds);

/// <summary>
/// Available slots grouped by site and date.
/// </summary>
public sealed record AvailableSlotGroup(
    Guid SiteId,
    string SiteName,
    DateOnly Date,
    IReadOnlyList<AvailableSlotInfo> Slots);

/// <summary>
/// A single available time slot on a court.
/// </summary>
public sealed record AvailableSlotInfo(
    Guid CourtId,
    string CourtLabel,
    DateTime StartAtUtc,
    DateTime EndAtUtc);

/// <summary>
/// Court utilization percentage for a given site.
/// </summary>
public sealed record CourtUtilizationInfo(
    Guid SiteId,
    Guid CourtId,
    string CourtLabel,
    decimal UtilizationPercentage);

/// <summary>
/// Raw suggestion returned by the AI service before enrichment.
/// </summary>
public sealed record RawSlotSuggestion(
    Guid SiteId,
    Guid CourtId,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    string Reason,
    string ConfidenceTag);
