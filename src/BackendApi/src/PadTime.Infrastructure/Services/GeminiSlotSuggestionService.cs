// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PadTime.Application.Common.Interfaces;

namespace PadTime.Infrastructure.Services;

/// <summary>
/// Calls the Google Gemini API to generate slot suggestions based on user context.
/// Returns an empty list on any failure (non-critical, advisory feature).
/// </summary>
public sealed class GeminiSlotSuggestionService : ISlotSuggestionService
{
    private static readonly Action<ILogger, Exception?> LogApiKeyMissing =
        LoggerMessage.Define(LogLevel.Warning, new EventId(1, nameof(LogApiKeyMissing)),
            "Gemini API key is not configured");

    private static readonly Action<ILogger, Exception?> LogEmptyResponse =
        LoggerMessage.Define(LogLevel.Warning, new EventId(2, nameof(LogEmptyResponse)),
            "Gemini returned empty response");

    private static readonly Action<ILogger, string, Exception?> LogApiFailed =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(3, nameof(LogApiFailed)),
            "Failed to get suggestions from Gemini API: {ErrorDetail}");

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiSlotSuggestionService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public GeminiSlotSuggestionService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<GeminiSlotSuggestionService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IReadOnlyList<RawSlotSuggestion>> GetSuggestionsAsync(
        SlotSuggestionContext context,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            LogApiKeyMissing(_logger, null);
            return [];
        }

        var model = _configuration["Gemini:Model"] ?? "gemini-2.5-flash";

        try
        {
            var prompt = BuildPrompt(context);
            var requestBody = BuildRequestBody(prompt);

            var client = _httpClientFactory.CreateClient("Gemini");
            client.DefaultRequestHeaders.Remove("x-goog-api-key");
            client.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";

            var response = await client.PostAsJsonAsync(url, requestBody, JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                LogApiFailed(_logger, $"HTTP {(int)response.StatusCode}: {errorBody}", null);
                return [];
            }

            var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiResponse>(JsonOptions, cancellationToken);

            var text = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                LogEmptyResponse(_logger, null);
                return [];
            }

            var jsonText = ExtractJson(text);
            var suggestions = JsonSerializer.Deserialize<List<GeminiSuggestion>>(jsonText, JsonOptions);
            if (suggestions is null || suggestions.Count == 0)
                return [];

            return suggestions
                .Where(s => s.SiteId != Guid.Empty && s.CourtId != Guid.Empty)
                .Take(3)
                .Select(s => new RawSlotSuggestion(
                    s.SiteId,
                    s.CourtId,
                    DateTime.Parse(s.StartAtUtc, CultureInfo.InvariantCulture).ToUniversalTime(),
                    DateTime.Parse(s.EndAtUtc, CultureInfo.InvariantCulture).ToUniversalTime(),
                    s.Reason ?? "Recommended slot",
                    NormalizeConfidenceTag(s.ConfidenceTag)))
                .ToList();
        }
        catch (Exception ex)
        {
            LogApiFailed(_logger, ex.Message, ex);
            return [];
        }
    }

    private static string BuildPrompt(SlotSuggestionContext context)
    {
        var patternJson = JsonSerializer.Serialize(new
        {
            dayFreq = context.PlayPattern.DayOfWeekFrequency
                .ToDictionary(kv => ((int)kv.Key).ToString(CultureInfo.InvariantCulture), kv => kv.Value),
            timeWindows = context.PlayPattern.PreferredTimeWindows,
            prefSites = context.PlayPattern.PreferredSiteIds,
            prefCourts = context.PlayPattern.PreferredCourtIds,
        }, JsonOptions);

        var slotsJson = JsonSerializer.Serialize(
            context.AvailableSlots.Select(g => new
            {
                site = g.SiteId,
                name = g.SiteName,
                date = g.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                slots = g.Slots.Select(s => new
                {
                    court = s.CourtId,
                    label = s.CourtLabel,
                    start = s.StartAtUtc.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                    end = s.EndAtUtc.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                }),
            }), JsonOptions);

        var utilizationJson = JsonSerializer.Serialize(
            context.CourtUtilization.Select(u => new
            {
                site = u.SiteId,
                court = u.CourtId,
                label = u.CourtLabel,
                pct = u.UtilizationPercentage,
            }), JsonOptions);

        return $"""
            Based on this padel player's history and available court slots, suggest exactly 3 optimal booking times.

            Player patterns: {patternJson}
            Available slots: {slotsJson}
            Court utilization (last 30 days): {utilizationJson}

            Rules:
            - Pick slots ONLY from the "available slots" list above (use exact site, court, start, end values)
            - Prefer the player's usual day-of-week and time-of-day patterns
            - Prefer courts the player has used before
            - Suggest diverse days (not all on the same date)
            - For each suggestion, explain in 1 short sentence why it's a good fit (in French)
            - Assign a confidenceTag: "strong_match" if it aligns with multiple preferences, "good_fit" if it aligns with some, "worth_trying" if it's an exploratory suggestion
            - If the player has no history, suggest popular/low-utilization slots

            Return ONLY a JSON array of exactly 3 objects with these fields:
            siteId (guid), courtId (guid), startAtUtc (ISO string), endAtUtc (ISO string), reason (string in French), confidenceTag (string)
            """;
    }

    private static object BuildRequestBody(string prompt)
    {
        return new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = prompt } },
                },
            },
            generationConfig = new
            {
                temperature = 0.3,
                maxOutputTokens = 2048,
                responseMimeType = "application/json",
                thinkingConfig = new { thinkingBudget = 0 },
            },
        };
    }

    private static string ExtractJson(string text)
    {
        // If text is wrapped in ```json ... ```, extract the inner content
        var trimmed = text.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNewline = trimmed.IndexOf('\n', StringComparison.Ordinal);
            if (firstNewline > 0)
                trimmed = trimmed[(firstNewline + 1)..];
            if (trimmed.EndsWith("```", StringComparison.Ordinal))
                trimmed = trimmed[..^3].Trim();
        }

        // Find the JSON array boundaries
        var start = trimmed.IndexOf('[');
        var end = trimmed.LastIndexOf(']');
        if (start >= 0 && end > start)
            return trimmed[start..(end + 1)];

        return trimmed;
    }

    private static string NormalizeConfidenceTag(string? tag) =>
        tag?.ToLowerInvariant() switch
        {
            "strong_match" => "strong_match",
            "good_fit" => "good_fit",
            "worth_trying" => "worth_trying",
            _ => "good_fit",
        };

    // --- Gemini response deserialization models ---

    private sealed class GeminiResponse
    {
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    private sealed class GeminiCandidate
    {
        public GeminiContent? Content { get; set; }
    }

    private sealed class GeminiContent
    {
        public List<GeminiPart>? Parts { get; set; }
    }

    private sealed class GeminiPart
    {
        public string? Text { get; set; }
    }

    private sealed class GeminiSuggestion
    {
        public Guid SiteId { get; set; }
        public Guid CourtId { get; set; }
        public string StartAtUtc { get; set; } = "";
        public string EndAtUtc { get; set; } = "";
        public string? Reason { get; set; }
        public string? ConfidenceTag { get; set; }
    }
}
