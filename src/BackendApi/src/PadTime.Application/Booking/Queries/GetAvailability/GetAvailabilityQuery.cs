// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;

namespace PadTime.Application.Booking.Queries.GetAvailability;

/// <summary>
/// Query to retrieve available booking slots for a site on a given date, optionally filtered by court.
/// </summary>
/// <param name="SiteId">Identifier of the site to check availability for.</param>
/// <param name="Date">The date to check.</param>
/// <param name="CourtId">Optional court filter. When omitted, all active courts are included.</param>
public sealed record GetAvailabilityQuery(
    Guid SiteId,
    DateOnly Date,
    Guid? CourtId = null) : IRequest<AvailabilityResult>;

/// <summary>
/// Result containing all time slots and their availability status for the requested site and date.
/// </summary>
public sealed record AvailabilityResult(
    Guid SiteId,
    DateOnly Date,
    IReadOnlyList<SlotAvailability> Slots);

/// <summary>
/// Represents a single time slot on a court with its booking availability.
/// </summary>
public sealed record SlotAvailability(
    Guid? CourtId,
    string? CourtLabel,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    bool Available);