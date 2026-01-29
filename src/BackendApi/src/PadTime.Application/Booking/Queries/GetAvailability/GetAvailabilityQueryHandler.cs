using MediatR;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Booking;
using PadTime.Domain.Common;

namespace PadTime.Application.Booking.Queries.GetAvailability;

public sealed class GetAvailabilityQueryHandler : IRequestHandler<GetAvailabilityQuery, AvailabilityResult>
{
    private readonly ISiteRepository _siteRepository;
    private readonly ICourtRepository _courtRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IClosureRepository _closureRepository;

    public GetAvailabilityQueryHandler(
        ISiteRepository siteRepository,
        ICourtRepository courtRepository,
        IMatchRepository matchRepository,
        IClosureRepository closureRepository)
    {
        _siteRepository = siteRepository;
        _courtRepository = courtRepository;
        _matchRepository = matchRepository;
        _closureRepository = closureRepository;
    }

    public async Task<AvailabilityResult> Handle(GetAvailabilityQuery request, CancellationToken cancellationToken)
    {
        // Check if site is closed
        var isClosed = await _closureRepository.IsClosedAsync(request.SiteId, request.Date, cancellationToken);
        if (isClosed)
        {
            return new AvailabilityResult(request.SiteId, request.Date, []);
        }

        // Get site with schedule
        var site = await _siteRepository.GetByIdWithSchedulesAsync(
            request.SiteId,
            request.Date.Year,
            cancellationToken);

        if (site is null)
        {
            return new AvailabilityResult(request.SiteId, request.Date, []);
        }

        var schedule = site.GetScheduleForYear(request.Date.Year);
        if (schedule is null)
        {
            return new AvailabilityResult(request.SiteId, request.Date, []);
        }

        // Get courts
        var courts = request.CourtId.HasValue
            ? [await _courtRepository.GetByIdAsync(request.CourtId.Value, cancellationToken)]
            : await _courtRepository.GetBySiteIdAsync(request.SiteId, cancellationToken);

        courts = courts.Where(c => c is not null && c.IsActive).ToList()!;

        // Get timezone for conversion
        var timezone = TimeZoneInfo.FindSystemTimeZoneById(site.Timezone);

        // Generate slots and check availability
        var slots = new List<SlotAvailability>();

        foreach (var court in courts)
        {
            if (court is null) continue;

            foreach (var timeSlot in schedule.GenerateSlots(request.Date))
            {
                var startUtc = timeSlot.ToUtcStart(timezone);
                var endUtc = timeSlot.ToUtcEnd(timezone);

                var isBooked = await _matchRepository.ExistsForSlotAsync(court.Id, startUtc, cancellationToken);

                slots.Add(new SlotAvailability(
                    court.Id,
                    court.Label,
                    startUtc,
                    endUtc,
                    Available: !isBooked));
            }
        }

        return new AvailabilityResult(request.SiteId, request.Date, slots);
    }
}
