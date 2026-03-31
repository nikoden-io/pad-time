// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Booking.Queries.GetSlotSuggestions;

/// <summary>
/// Returns AI-generated slot suggestions for the current authenticated user.
/// </summary>
public sealed record GetSlotSuggestionsQuery() : IRequest<Result<GetSlotSuggestionsResponse>>;

/// <summary>
/// Response containing AI-generated slot suggestions.
/// </summary>
public sealed record GetSlotSuggestionsResponse(
    IReadOnlyList<SlotSuggestionDto> Suggestions,
    DateTime GeneratedAtUtc,
    bool FallbackUsed);

/// <summary>
/// A single slot suggestion with site/court details and AI reasoning.
/// </summary>
public sealed record SlotSuggestionDto(
    Guid SiteId,
    string SiteName,
    Guid CourtId,
    string CourtLabel,
    DateOnly Date,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    string Reason,
    string ConfidenceTag);
