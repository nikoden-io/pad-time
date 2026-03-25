using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Admin.Queries.GetSiteOverview;

public sealed record GetSiteOverviewQuery(Guid SiteId) : IRequest<Result<SiteOverviewDto>>;

public sealed record SiteOverviewDto(Guid SiteId, IReadOnlyList<SiteAlertDto> Alerts);

public sealed record SiteAlertDto(string Type, string Description, object? Payload = null);
