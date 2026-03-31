// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using MediatR;
using PadTime.Domain.Common;

namespace PadTime.Application.Admin.Queries.GetAiTrends;

/// <summary>
/// Returns AI-generated business trend insights for the admin dashboard.
/// </summary>
public sealed record GetAiTrendsQuery(Guid? SiteId) : IRequest<Result<AiTrendsResponse>>;

/// <summary>
/// AI-generated trends response with categorised insights.
/// </summary>
public sealed record AiTrendsResponse(
    IReadOnlyList<AiTrendDto> Trends,
    DateTime GeneratedAtUtc,
    bool FallbackUsed);

/// <summary>
/// A single trend insight produced by the AI.
/// </summary>
public sealed record AiTrendDto(
    string Category,
    string Title,
    string Description,
    string Impact,
    string Icon);
