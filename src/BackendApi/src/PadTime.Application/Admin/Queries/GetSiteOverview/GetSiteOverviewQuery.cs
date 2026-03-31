// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Admin.Queries.GetSiteOverview;

/// <summary>
/// Query to retrieve the operational overview of a site, including active alerts.
/// </summary>
/// <param name="SiteId">Unique identifier of the site.</param>
public sealed record GetSiteOverviewQuery(Guid SiteId) : IRequest<Result<SiteOverviewDto>>;

/// <summary>
/// Site overview containing operational alerts such as unprocessed matches, unpaid participants, and organizer debts.
/// </summary>
public sealed record SiteOverviewDto(Guid SiteId, IReadOnlyList<SiteAlertDto> Alerts);

/// <summary>
/// An individual alert raised for a site, with a type key, description, and optional payload.
/// </summary>
public sealed record SiteAlertDto(string Type, string Description, object? Payload = null);