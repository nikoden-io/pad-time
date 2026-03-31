// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using Microsoft.EntityFrameworkCore;
using PadTime.Application.Common.Interfaces.Repositories;

namespace PadTime.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for site-level statistical queries including booking counts,
/// court utilization rates, and daily booking statistics for dashboard displays.
/// </summary>
public sealed class SiteStatisticsRepository(PadTimeDbContext context) : ISiteStatisticsRepository
{
    /// <inheritdoc />
    public async Task<int> GetBookingCountForPeriodAsync(
        Guid siteId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        return await context.Matches
            .Join(context.Courts,
                match => match.CourtId,
                court => court.Id,
                (match, court) => new { match, court })
            .Where(x => x.court.SiteId == siteId &&
                       x.match.StartAtUtc >= startDate &&
                       x.match.StartAtUtc < endDate)
            .CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetUpcomingBookingCountAsync(
        Guid siteId, 
        DateOnly startDate, 
        DateOnly endDate, 
        CancellationToken cancellationToken = default)
    {
        var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
        var endDateTime = endDate.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var now = DateTime.UtcNow;

        return await context.Matches
            .Join(context.Courts,
                match => match.CourtId,
                court => court.Id,
                (match, court) => new { match, court })
            .Where(x => x.court.SiteId == siteId &&
                       x.match.StartAtUtc >= now &&
                       x.match.StartAtUtc >= startDateTime &&
                       x.match.StartAtUtc < endDateTime)
            .CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DateTime> GetLastBookingDateAsync(
        Guid siteId, 
        CancellationToken cancellationToken = default)
    {
        var lastBooking = await context.Matches
            .Join(context.Courts,
                match => match.CourtId,
                court => court.Id,
                (match, court) => new { match, court })
            .Where(x => x.court.SiteId == siteId)
            .OrderByDescending(x => x.match.StartAtUtc)
            .Select(x => x.match.StartAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return lastBooking == default ? DateTime.MinValue : lastBooking;
    }

    /// <inheritdoc />
    public async Task<List<CourtUtilizationStats>> GetCourtUtilizationAsync(
        Guid siteId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        // Calculate total possible slots per court for the period
        var totalDays = (endDate - startDate).Days;
        var slotsPerDay = 8; // Assuming 8 slots per day (can be made configurable)
        var totalPossibleSlots = totalDays * slotsPerDay;

        var courtStats = await context.Courts
            .Where(c => c.SiteId == siteId && c.IsActive)
            .GroupJoin(
                context.Matches.Where(m => m.StartAtUtc >= startDate && m.StartAtUtc < endDate),
                court => court.Id,
                match => match.CourtId,
                (court, matches) => new { court, matches })
            .Select(x => new CourtUtilizationStats(
                CourtId: x.court.Id,
                CourtLabel: x.court.Label,
                BookingCount: x.matches.Count(),
                UtilizationPercentage: totalPossibleSlots > 0 
                    ? (decimal)x.matches.Count() / totalPossibleSlots * 100 
                    : 0))
            .ToListAsync(cancellationToken);

        return courtStats;
    }

    /// <inheritdoc />
    public async Task<List<DailyBookingStats>> GetDailyBookingStatsAsync(
        Guid siteId, 
        DateOnly startDate, 
        DateOnly endDate, 
        CancellationToken cancellationToken = default)
    {
        var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
        var endDateTime = endDate.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var dailyStats = await context.Matches
            .Join(context.Courts,
                match => match.CourtId,
                court => court.Id,
                (match, court) => new { match, court })
            .Where(x => x.court.SiteId == siteId &&
                       x.match.StartAtUtc >= startDateTime &&
                       x.match.StartAtUtc < endDateTime)
            .GroupBy(x => DateOnly.FromDateTime(x.match.StartAtUtc))
            .Select(g => new DailyBookingStats(
                Date: g.Key,
                BookingCount: g.Count(),
                UniqueUsers: g.Select(x => x.match.OrganizerId).Distinct().Count()))
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        return dailyStats;
    }
}