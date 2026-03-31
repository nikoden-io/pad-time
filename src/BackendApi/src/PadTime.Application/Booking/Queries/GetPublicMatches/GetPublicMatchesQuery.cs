// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Booking.Queries.GetPublicMatches;

/// <summary>
/// Query to retrieve public matches with available seats, optionally filtered by site and date range.
/// </summary>
/// <param name="SiteId">Optional site filter.</param>
/// <param name="FromUtc">Optional start of date range (defaults to now).</param>
/// <param name="ToUtc">Optional end of date range (defaults to 30 days from now).</param>
/// <param name="Page">Page number (1-based).</param>
/// <param name="PageSize">Number of items per page.</param>
public sealed record GetPublicMatchesQuery(
    Guid? SiteId,
    DateTime? FromUtc,
    DateTime? ToUtc,
    int Page,
    int PageSize) : IRequest<Result<IReadOnlyList<PublicMatchDto>>>;

/// <summary>
/// DTO representing a public match with participant count and available seats.
/// </summary>
public sealed record PublicMatchDto(
    Guid MatchId,
    Guid SiteId,
    Guid CourtId,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    string Status,
    Guid OrganizerId,
    int PriceTotalCents,
    int ParticipantCount,
    int AvailableSeats,
    IReadOnlyList<ParticipantSummaryDto> Participants);

/// <summary>
/// Summary of a participant in a public match listing.
/// </summary>
public sealed record ParticipantSummaryDto(
    Guid MemberId,
    string Matricule,
    string Role,
    string PaymentStatus);