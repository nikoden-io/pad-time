// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;

namespace PadTime.Application.Sites.Queries.GetSiteStatistics;

/// <summary>
/// Handler for GetSiteStatisticsQuery.
/// Retrieves comprehensive site statistics for dashboard display.
/// </summary>
public sealed class GetSiteStatisticsQueryHandler(
    ISiteRepository siteRepository,
    ISiteStatisticsRepository statisticsRepository) 
    : IRequestHandler<GetSiteStatisticsQuery, Result<SiteStatisticsDto>>
{
    public async Task<Result<SiteStatisticsDto>> Handle(
        GetSiteStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var site = await siteRepository.GetByIdAsync(request.SiteId, cancellationToken);
        if (site is null)
        {
            return DomainErrors.Site.NotFound;
        }

        var now = DateTime.UtcNow;
        var currentMonth = new DateTime(now.Year, now.Month, 1);
        var lastMonth = currentMonth.AddMonths(-1);
        var today = DateOnly.FromDateTime(now);
        var weekFromNow = today.AddDays(7);

        // Get booking statistics
        var bookingsThisMonth = await statisticsRepository.GetBookingCountForPeriodAsync(
            request.SiteId, currentMonth, now, cancellationToken);
        
        var bookingsLastMonth = await statisticsRepository.GetBookingCountForPeriodAsync(
            request.SiteId, lastMonth, currentMonth, cancellationToken);

        var upcomingBookingsToday = await statisticsRepository.GetUpcomingBookingCountAsync(
            request.SiteId, today, today, cancellationToken);
        
        var upcomingBookingsThisWeek = await statisticsRepository.GetUpcomingBookingCountAsync(
            request.SiteId, today, weekFromNow, cancellationToken);

        var lastBookingDate = await statisticsRepository.GetLastBookingDateAsync(
            request.SiteId, cancellationToken);

        // Calculate growth percentage
        var growthPercentage = bookingsLastMonth > 0 
            ? ((decimal)(bookingsThisMonth - bookingsLastMonth) / bookingsLastMonth) * 100
            : bookingsThisMonth > 0 ? 100 : 0;

        // Get court utilization
        var courtUtilization = await statisticsRepository.GetCourtUtilizationAsync(
            request.SiteId, currentMonth, now, cancellationToken);

        // Get recent booking stats (last 30 days)
        var thirtyDaysAgo = today.AddDays(-30);
        var recentBookingStats = await statisticsRepository.GetDailyBookingStatsAsync(
            request.SiteId, thirtyDaysAgo, today, cancellationToken);

        var statisticsDto = new SiteStatisticsDto(
            SiteId: site.Id,
            SiteName: site.Name,
            TotalCourts: site.Courts.Count,
            ActiveCourts: site.Courts.Count(c => c.IsActive),
            TotalBookingsThisMonth: bookingsThisMonth,
            TotalBookingsLastMonth: bookingsLastMonth,
            BookingGrowthPercentage: Math.Round(growthPercentage, 1),
            UpcomingBookingsToday: upcomingBookingsToday,
            UpcomingBookingsThisWeek: upcomingBookingsThisWeek,
            LastBookingDate: lastBookingDate,
            CourtUtilization: courtUtilization
                .Select(cu => new CourtUtilizationDto(
                    CourtId: cu.CourtId,
                    CourtLabel: cu.CourtLabel,
                    BookingsThisMonth: cu.BookingCount,
                    UtilizationPercentage: Math.Round(cu.UtilizationPercentage, 1)))
                .ToList(),
            RecentBookingStats: recentBookingStats
                .Select(rbs => new DailyBookingStatsDto(
                    Date: rbs.Date,
                    BookingCount: rbs.BookingCount,
                    UniqueUsers: rbs.UniqueUsers))
                .ToList()
        );

        return statisticsDto;
    }
}