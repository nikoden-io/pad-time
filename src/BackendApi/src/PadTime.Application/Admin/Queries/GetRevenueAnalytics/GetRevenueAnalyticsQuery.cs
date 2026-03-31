// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Admin.Queries.GetRevenueAnalytics;

/// <summary>
/// Query to retrieve revenue analytics for a date range, optionally scoped to a specific site.
/// </summary>
/// <param name="SiteId">Optional site filter. Site admins are automatically scoped to their own site.</param>
/// <param name="FromUtc">Start of the date range (UTC).</param>
/// <param name="ToUtc">End of the date range (UTC).</param>
public sealed record GetRevenueAnalyticsQuery(
    Guid? SiteId,
    DateTime FromUtc,
    DateTime ToUtc) : IRequest<Result<RevenueAnalyticsDto>>;

/// <summary>
/// Revenue analytics result containing daily revenue breakdowns per site.
/// </summary>
public sealed record RevenueAnalyticsDto(
    DateTime From,
    DateTime To,
    string Currency,
    IReadOnlyList<RevenueItemDto> Items);

/// <summary>
/// A single revenue data point representing aggregated payments for a specific date and site.
/// </summary>
public sealed record RevenueItemDto(
    DateOnly Date,
    Guid SiteId,
    int AmountCents,
    int PaymentCount);