// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Queries.GetSiteStatistics;

/// <summary>
/// Query to get site statistics for dashboard display.
/// </summary>
public sealed record GetSiteStatisticsQuery(Guid SiteId) : IRequest<Result<SiteStatisticsDto>>;

/// <summary>
/// DTO representing site statistics for dashboard.
/// </summary>
public sealed record SiteStatisticsDto(
    Guid SiteId,
    string SiteName,
    int TotalCourts,
    int ActiveCourts,
    int TotalBookingsThisMonth,
    int TotalBookingsLastMonth,
    decimal BookingGrowthPercentage,
    int UpcomingBookingsToday,
    int UpcomingBookingsThisWeek,
    DateTime LastBookingDate,
    IReadOnlyList<CourtUtilizationDto> CourtUtilization,
    IReadOnlyList<DailyBookingStatsDto> RecentBookingStats
);

/// <summary>
/// DTO representing court utilization statistics.
/// </summary>
public sealed record CourtUtilizationDto(
    Guid CourtId,
    string CourtLabel,
    int BookingsThisMonth,
    decimal UtilizationPercentage
);

/// <summary>
/// DTO representing daily booking statistics.
/// </summary>
public sealed record DailyBookingStatsDto(
    DateOnly Date,
    int BookingCount,
    int UniqueUsers
);