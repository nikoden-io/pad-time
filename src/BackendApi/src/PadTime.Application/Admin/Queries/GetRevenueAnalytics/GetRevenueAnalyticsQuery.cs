using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Admin.Queries.GetRevenueAnalytics;

public sealed record GetRevenueAnalyticsQuery(
    Guid? SiteId,
    DateTime FromUtc,
    DateTime ToUtc) : IRequest<Result<RevenueAnalyticsDto>>;

public sealed record RevenueAnalyticsDto(
    DateTime From,
    DateTime To,
    string Currency,
    IReadOnlyList<RevenueItemDto> Items);

public sealed record RevenueItemDto(
    DateOnly Date,
    Guid SiteId,
    int AmountCents,
    int PaymentCount);
