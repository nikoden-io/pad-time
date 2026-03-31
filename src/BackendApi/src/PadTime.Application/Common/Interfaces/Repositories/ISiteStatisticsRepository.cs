// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
namespace PadTime.Application.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for site statistics operations.
/// </summary>
public interface ISiteStatisticsRepository
{
    /// <summary>
    /// Gets the total number of bookings for a site within a date range.
    /// </summary>
    Task<int> GetBookingCountForPeriodAsync(
        Guid siteId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the number of upcoming bookings for a site within a date range.
    /// </summary>
    Task<int> GetUpcomingBookingCountAsync(
        Guid siteId, 
        DateOnly startDate, 
        DateOnly endDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the date of the last booking for a site.
    /// </summary>
    Task<DateTime> GetLastBookingDateAsync(
        Guid siteId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets court utilization statistics for a site within a date range.
    /// </summary>
    Task<List<CourtUtilizationStats>> GetCourtUtilizationAsync(
        Guid siteId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets daily booking statistics for a site within a date range.
    /// </summary>
    Task<List<DailyBookingStats>> GetDailyBookingStatsAsync(
        Guid siteId, 
        DateOnly startDate, 
        DateOnly endDate, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Court utilization statistics data.
/// </summary>
public sealed record CourtUtilizationStats(
    Guid CourtId,
    string CourtLabel,
    int BookingCount,
    decimal UtilizationPercentage
);

/// <summary>
/// Daily booking statistics data.
/// </summary>
public sealed record DailyBookingStats(
    DateOnly Date,
    int BookingCount,
    int UniqueUsers
);