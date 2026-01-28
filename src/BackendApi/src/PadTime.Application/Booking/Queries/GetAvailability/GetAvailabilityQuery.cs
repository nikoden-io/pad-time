using MediatR;

namespace PadTime.Application.Booking.Queries.GetAvailability;

public sealed record GetAvailabilityQuery(
    Guid SiteId,
    DateOnly Date,
    Guid? CourtId = null) : IRequest<AvailabilityResult>;

public sealed record AvailabilityResult(
    Guid SiteId,
    DateOnly Date,
    IReadOnlyList<SlotAvailability> Slots);

public sealed record SlotAvailability(
    Guid? CourtId,
    string? CourtLabel,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    bool Available);
