// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Booking.Queries.GetSiteMatches;

/// <summary>
/// Query to retrieve all matches for a specific site within an optional date range (admin use).
/// </summary>
/// <param name="SiteId">Identifier of the site.</param>
/// <param name="FromUtc">Optional start of date range.</param>
/// <param name="ToUtc">Optional end of date range.</param>
/// <param name="Page">Page number (1-based).</param>
/// <param name="PageSize">Number of items per page.</param>
public sealed record GetSiteMatchesQuery(
    Guid SiteId,
    DateTime? FromUtc,
    DateTime? ToUtc,
    int Page,
    int PageSize) : IRequest<Result<IReadOnlyList<SiteMatchDto>>>;

/// <summary>
/// Summary DTO of a match within a site listing, including type, status, and participant count.
/// </summary>
public sealed record SiteMatchDto(
    Guid MatchId,
    Guid SiteId,
    Guid CourtId,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    string Type,
    string Status,
    Guid OrganizerId,
    int PriceTotalCents,
    int ParticipantCount);