// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using System.Globalization;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;

namespace PadTime.Application.Admin.Queries.GetAiTrends;

/// <summary>
/// Gathers site statistics, revenue data, and member metrics, then delegates to
/// <see cref="IAiCompletionService"/> for AI-generated business insights.
/// </summary>
public sealed class GetAiTrendsQueryHandler
    : IRequestHandler<GetAiTrendsQuery, Result<AiTrendsResponse>>
{
    private static readonly Action<ILogger, Exception?> LogTrendFailure =
        LoggerMessage.Define(LogLevel.Warning, new EventId(1, nameof(LogTrendFailure)),
            "Failed to generate AI trends");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private readonly ISiteRepository _siteRepository;
    private readonly ISiteStatisticsRepository _statisticsRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IAiCompletionService _aiService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<GetAiTrendsQueryHandler> _logger;

    public GetAiTrendsQueryHandler(
        ISiteRepository siteRepository,
        ISiteStatisticsRepository statisticsRepository,
        IPaymentRepository paymentRepository,
        IMemberRepository memberRepository,
        ICurrentUser currentUser,
        IAiCompletionService aiService,
        IDateTimeProvider dateTimeProvider,
        ILogger<GetAiTrendsQueryHandler> logger)
    {
        _siteRepository = siteRepository;
        _statisticsRepository = statisticsRepository;
        _paymentRepository = paymentRepository;
        _memberRepository = memberRepository;
        _currentUser = currentUser;
        _aiService = aiService;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result<AiTrendsResponse>> Handle(
        GetAiTrendsQuery request, CancellationToken cancellationToken)
    {
        var now = _dateTimeProvider.UtcNow;

        try
        {
            var effectiveSiteId = request.SiteId;
            if (_currentUser.IsSiteAdmin && !_currentUser.IsGlobalAdmin)
                effectiveSiteId = _currentUser.SiteId;

            var allSites = await _siteRepository.GetAllActiveAsync(cancellationToken);
            var sites = effectiveSiteId.HasValue
                ? allSites.Where(s => s.Id == effectiveSiteId.Value).ToList()
                : allSites;

            if (sites.Count == 0)
                return EmptyResponse(now);

            // Gather analytics context
            var thirtyDaysAgo = now.AddDays(-30);
            var sixtyDaysAgo = now.AddDays(-60);
            var today = DateOnly.FromDateTime(now);
            var sevenDaysAgo = today.AddDays(-7);

            var siteData = new List<object>();

            foreach (var site in sites)
            {
                var bookingsLast30 = await _statisticsRepository.GetBookingCountForPeriodAsync(
                    site.Id, thirtyDaysAgo, now, cancellationToken);
                var bookingsPrev30 = await _statisticsRepository.GetBookingCountForPeriodAsync(
                    site.Id, sixtyDaysAgo, thirtyDaysAgo, cancellationToken);
                var courtUtil = await _statisticsRepository.GetCourtUtilizationAsync(
                    site.Id, thirtyDaysAgo, now, cancellationToken);
                var dailyStats = await _statisticsRepository.GetDailyBookingStatsAsync(
                    site.Id, sevenDaysAgo, today, cancellationToken);

                siteData.Add(new
                {
                    site = site.Name,
                    bookingsLast30days = bookingsLast30,
                    bookingsPrevious30days = bookingsPrev30,
                    growthPct = bookingsPrev30 > 0
                        ? Math.Round((bookingsLast30 - bookingsPrev30) * 100.0 / bookingsPrev30, 1)
                        : 0,
                    courtUtilization = courtUtil.Select(c => new { court = c.CourtLabel, pct = c.UtilizationPercentage }),
                    dailyBookingsLast7days = dailyStats.Select(d => new
                    {
                        date = d.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                        bookings = d.BookingCount,
                        uniqueUsers = d.UniqueUsers,
                    }),
                });
            }

            var revenueRows = await _paymentRepository.GetPaidBySiteAndDateRangeAsync(
                effectiveSiteId, thirtyDaysAgo, now, cancellationToken);

            var members = await _memberRepository.GetPagedAsync(1, 1, null, null, null, cancellationToken);

            var contextJson = JsonSerializer.Serialize(new
            {
                sites = siteData,
                revenueLast30Days = new
                {
                    totalEuros = revenueRows.Sum(r => r.Payment.AmountCents) / 100.0,
                    paymentCount = revenueRows.Count,
                },
                totalActiveMembers = members.TotalCount,
                today = now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            }, JsonOptions);

            var prompt = $"""
                Tu es un analyste business pour une plateforme de réservation de terrains de padel.
                Voici les données des 30 derniers jours :

                {contextJson}

                Analyse ces données et génère exactement 4 insights business en français, catégorisés ainsi :
                1. "revenue" — Tendance des revenus (croissance, baisse, stabilité)
                2. "usage" — Tendance d'utilisation des terrains (courts populaires, sous-utilisés)
                3. "members" — Tendance des membres (engagement, croissance, rétention)
                4. "opportunity" — Opportunité business identifiée (action concrète à prendre)

                Pour chaque insight :
                - "title" : titre court et percutant (max 8 mots)
                - "description" : explication actionnable en 2 phrases, adressée à l'admin avec "vous"
                - "impact" : "positive", "negative", ou "neutral"
                - "icon" : un emoji pertinent

                Retourne UNIQUEMENT un tableau JSON de 4 objets avec les champs :
                category (string), title (string), description (string), impact (string), icon (string)
                """;

            var json = await _aiService.CompleteJsonAsync(prompt, cancellationToken);
            if (json is null)
                return EmptyResponse(now);

            var parsed = JsonSerializer.Deserialize<List<AiTrendRaw>>(json, JsonOptions);
            if (parsed is null || parsed.Count == 0)
                return EmptyResponse(now);

            var trends = parsed
                .Take(4)
                .Select(t => new AiTrendDto(
                    t.Category ?? "general",
                    t.Title ?? "",
                    t.Description ?? "",
                    NormalizeImpact(t.Impact),
                    t.Icon ?? "📊"))
                .ToList();

            return new AiTrendsResponse(trends, now, FallbackUsed: false);
        }
        catch (Exception ex)
        {
            LogTrendFailure(_logger, ex);
            return EmptyResponse(now);
        }
    }

    private static string NormalizeImpact(string? impact) =>
        impact?.ToLowerInvariant() switch
        {
            "positive" => "positive",
            "negative" => "negative",
            "neutral" => "neutral",
            _ => "neutral",
        };

    private static AiTrendsResponse EmptyResponse(DateTime now) =>
        new([], now, FallbackUsed: true);

    private sealed class AiTrendRaw
    {
        public string? Category { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Impact { get; set; }
        public string? Icon { get; set; }
    }
}
