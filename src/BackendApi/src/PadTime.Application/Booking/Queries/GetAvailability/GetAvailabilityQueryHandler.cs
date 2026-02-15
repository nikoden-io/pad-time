using MediatR;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Site;

namespace PadTime.Application.Booking.Queries.GetAvailability;

public sealed class GetAvailabilityQueryHandler : IRequestHandler<GetAvailabilityQuery, AvailabilityResult>
{
    private readonly ISiteRepository _siteRepository;
    private readonly ICourtRepository _courtRepository;
    private readonly IMatchRepository _matchRepository;

    public GetAvailabilityQueryHandler(
        ISiteRepository siteRepository,
        ICourtRepository courtRepository,
        IMatchRepository matchRepository)
    {
        _siteRepository = siteRepository;
        _courtRepository = courtRepository;
        _matchRepository = matchRepository;
    }

    public async Task<AvailabilityResult> Handle(GetAvailabilityQuery request, CancellationToken cancellationToken)
    {
        // Get site with schedules and closures
        var site = await _siteRepository.GetByIdWithSchedulesAndClosuresAsync(
            request.SiteId,
            cancellationToken);

        if (site is null)
        {
            return new AvailabilityResult(request.SiteId, request.Date, []);
        }

        // Check if site is closed
        if (site.IsClosedOn(request.Date))
        {
            return new AvailabilityResult(request.SiteId, request.Date, []);
        }

        // Get courts
        List<Court> courts;
        if (request.CourtId.HasValue)
        {
            var court = await _courtRepository.GetByIdAsync(request.CourtId.Value, cancellationToken);
            courts = court is not null ? [court] : [];
        }
        else
        {
            courts = await _courtRepository.GetBySiteIdAsync(request.SiteId, cancellationToken);
        }

        courts = [.. courts.Where(c => c.IsActive)];

        // Get timezone for conversion
        var timezone = site.GetTimeZone();

        // Generate slots and check availability
        var slots = new List<SlotAvailability>();

        foreach (var court in courts)
        {
            if (court is null) continue;

            // Skip if court is closed
            if (site.IsCourtClosedOn(court.Id, request.Date))
                continue;

            foreach (var timeSlot in site.GetAvailableSlots(request.Date, court.Id))
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
