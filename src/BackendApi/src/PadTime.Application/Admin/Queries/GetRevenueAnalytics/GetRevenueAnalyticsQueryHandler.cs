using MediatR;
using PadTime.Application.Common.Interfaces;
using PadTime.Application.Common.Interfaces.Repositories;
using PadTime.Domain.Common;

namespace PadTime.Application.Admin.Queries.GetRevenueAnalytics;

public sealed class GetRevenueAnalyticsQueryHandler
    : IRequestHandler<GetRevenueAnalyticsQuery, Result<RevenueAnalyticsDto>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ICurrentUser _currentUser;

    public GetRevenueAnalyticsQueryHandler(
        IPaymentRepository paymentRepository,
        ICurrentUser currentUser)
    {
        _paymentRepository = paymentRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<RevenueAnalyticsDto>> Handle(
        GetRevenueAnalyticsQuery request,
        CancellationToken cancellationToken)
    {
        // site admins are restricted to their own site
        var effectiveSiteId = request.SiteId;
        if (_currentUser.IsSiteAdmin && !_currentUser.IsGlobalAdmin)
            effectiveSiteId = _currentUser.SiteId;

        var rows = await _paymentRepository.GetPaidBySiteAndDateRangeAsync(
            effectiveSiteId,
            request.FromUtc,
            request.ToUtc,
            cancellationToken);

        var items = rows
            .GroupBy(r => (Date: DateOnly.FromDateTime(r.Payment.CreatedAtUtc), r.SiteId))
            .Select(g => new RevenueItemDto(
                g.Key.Date,
                g.Key.SiteId,
                g.Sum(r => r.Payment.AmountCents),
                g.Count()))
            .OrderBy(i => i.Date)
            .ThenBy(i => i.SiteId)
            .ToList();

        return new RevenueAnalyticsDto(request.FromUtc, request.ToUtc, "EUR", items);
    }
}
