// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using Microsoft.Extensions.Logging;
using PadTime.Application.Booking.Queries.GetAvailability;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;
using PadTime.Domain.Members;

namespace PadTime.Application.Booking.Queries.GetSlotSuggestions;

/// <summary>
/// Handles <see cref="GetSlotSuggestionsQuery"/> by gathering user history, available slots,
/// and court utilization data, then delegating to <see cref="ISlotSuggestionService"/> for AI analysis.
/// </summary>
public sealed class GetSlotSuggestionsQueryHandler
    : IRequestHandler<GetSlotSuggestionsQuery, Result<GetSlotSuggestionsResponse>>
{
    private static readonly Action<ILogger, Exception?> LogSuggestionFailure =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(1, nameof(LogSuggestionFailure)),
            "Failed to generate slot suggestions");

    private readonly ICurrentUser _currentUser;
    private readonly IMemberRepository _memberRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly ISiteRepository _siteRepository;
    private readonly ISiteStatisticsRepository _statisticsRepository;
    private readonly ISlotSuggestionService _suggestionService;
    private readonly IMediator _mediator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<GetSlotSuggestionsQueryHandler> _logger;

    private const int HistoryMatchCount = 20;
    private const int LookAheadDays = 7;
    private const int MaxSlotsPerGroup = 12;

    public GetSlotSuggestionsQueryHandler(
        ICurrentUser currentUser,
        IMemberRepository memberRepository,
        IMatchRepository matchRepository,
        ISiteRepository siteRepository,
        ISiteStatisticsRepository statisticsRepository,
        ISlotSuggestionService suggestionService,
        IMediator mediator,
        IDateTimeProvider dateTimeProvider,
        ILogger<GetSlotSuggestionsQueryHandler> logger)
    {
        _currentUser = currentUser;
        _memberRepository = memberRepository;
        _matchRepository = matchRepository;
        _siteRepository = siteRepository;
        _statisticsRepository = statisticsRepository;
        _suggestionService = suggestionService;
        _mediator = mediator;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result<GetSlotSuggestionsResponse>> Handle(
        GetSlotSuggestionsQuery request,
        CancellationToken cancellationToken)
    {
        var now = _dateTimeProvider.UtcNow;

        try
        {
            // 1. Resolve current member
            var member = await _memberRepository.GetBySubjectAsync(_currentUser.Subject, cancellationToken);
            if (member is null)
                return EmptyResponse(now, fallback: true);

            // 2. Fetch match history
            var matches = await _matchRepository.GetByMemberIdAsync(
                member.Id, fromUtc: null, page: 1, pageSize: HistoryMatchCount, cancellationToken);

            // 3. Compute play pattern from history
            var playPattern = BuildPlayPattern(matches);

            // 4. Get accessible sites
            var allSites = await _siteRepository.GetAllActiveAsync(cancellationToken);
            var accessibleSites = member.Category == MemberCategory.Site && member.SiteId.HasValue
                ? allSites.Where(s => s.Id == member.SiteId.Value).ToList()
                : allSites;

            if (accessibleSites.Count == 0)
                return EmptyResponse(now, fallback: true);

            // 5. Get available slots for the next N days
            var slotGroups = new List<AvailableSlotGroup>();
            var today = DateOnly.FromDateTime(now);

            foreach (var site in accessibleSites)
            {
                for (var d = 0; d < LookAheadDays; d++)
                {
                    var date = today.AddDays(d);
                    var availability = await _mediator.Send(
                        new GetAvailabilityQuery(site.Id, date), cancellationToken);

                    var availableSlots = availability.Slots
                        .Where(s => s.Available && s.StartAtUtc > now)
                        .Take(MaxSlotsPerGroup)
                        .Select(s => new AvailableSlotInfo(
                            s.CourtId!.Value, s.CourtLabel!, s.StartAtUtc, s.EndAtUtc))
                        .ToList();

                    if (availableSlots.Count > 0)
                    {
                        slotGroups.Add(new AvailableSlotGroup(
                            site.Id, site.Name, date, availableSlots));
                    }
                }
            }

            if (slotGroups.Count == 0)
                return EmptyResponse(now, fallback: true);

            // 6. Get court utilization for context
            var utilization = new List<CourtUtilizationInfo>();
            var thirtyDaysAgo = now.AddDays(-30);

            foreach (var site in accessibleSites)
            {
                var stats = await _statisticsRepository.GetCourtUtilizationAsync(
                    site.Id, thirtyDaysAgo, now, cancellationToken);

                utilization.AddRange(stats.Select(s => new CourtUtilizationInfo(
                    site.Id, s.CourtId, s.CourtLabel, s.UtilizationPercentage)));
            }

            // 7. Call AI suggestion service
            var context = new SlotSuggestionContext(playPattern, slotGroups, utilization);
            var rawSuggestions = await _suggestionService.GetSuggestionsAsync(context, cancellationToken);

            if (rawSuggestions.Count == 0)
                return EmptyResponse(now, fallback: true);

            // 8. Map raw suggestions to enriched DTOs
            var siteLookup = accessibleSites.ToDictionary(s => s.Id);
            var suggestions = new List<SlotSuggestionDto>();

            foreach (var raw in rawSuggestions)
            {
                var siteName = siteLookup.TryGetValue(raw.SiteId, out var s) ? s.Name : "Unknown";

                // Find court label from the slot groups
                var courtLabel = slotGroups
                    .SelectMany(g => g.Slots)
                    .FirstOrDefault(sl => sl.CourtId == raw.CourtId)?.CourtLabel ?? "Unknown";

                suggestions.Add(new SlotSuggestionDto(
                    raw.SiteId,
                    siteName,
                    raw.CourtId,
                    courtLabel,
                    DateOnly.FromDateTime(raw.StartAtUtc),
                    raw.StartAtUtc,
                    raw.EndAtUtc,
                    raw.Reason,
                    raw.ConfidenceTag));
            }

            return new GetSlotSuggestionsResponse(suggestions, now, FallbackUsed: false);
        }
        catch (Exception ex)
        {
            LogSuggestionFailure(_logger, ex);
            return EmptyResponse(now, fallback: true);
        }
    }

    private static GetSlotSuggestionsResponse EmptyResponse(DateTime now, bool fallback) =>
        new([], now, fallback);

    private static MemberPlayPattern BuildPlayPattern(List<Domain.Booking.Match> matches)
    {
        if (matches.Count == 0)
        {
            return new MemberPlayPattern(
                new Dictionary<DayOfWeek, int>(),
                [],
                [],
                []);
        }

        // Day-of-week frequency
        var dayFreq = matches
            .GroupBy(m => m.StartAtUtc.DayOfWeek)
            .ToDictionary(g => g.Key, g => g.Count());

        // Time window clustering (round to nearest hour)
        var timeWindows = matches
            .GroupBy(m => $"{m.StartAtUtc.Hour:D2}:00-{m.EndAtUtc.Hour:D2}:00")
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key)
            .ToList();

        // Preferred sites (by frequency)
        var prefSites = matches
            .GroupBy(m => m.SiteId)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key)
            .ToList();

        // Preferred courts (by frequency)
        var prefCourts = matches
            .GroupBy(m => m.CourtId)
            .OrderByDescending(g => g.Count())
            .Take(4)
            .Select(g => g.Key)
            .ToList();

        return new MemberPlayPattern(dayFreq, timeWindows, prefSites, prefCourts);
    }
}
