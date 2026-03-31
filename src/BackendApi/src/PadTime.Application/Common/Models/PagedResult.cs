// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
namespace PadTime.Application.Common.Models;

/// <summary>
/// Generic paginated result wrapper.
/// </summary>
public sealed class PagedResult<T>
{
    /// <summary>Items on the current page.</summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>Current page number (1-based).</summary>
    public int Page { get; }

    /// <summary>Maximum number of items per page.</summary>
    public int PageSize { get; }

    /// <summary>Total number of items across all pages.</summary>
    public int TotalCount { get; }

    /// <summary>Total number of pages.</summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>Indicates whether a previous page exists.</summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>Indicates whether a next page exists.</summary>
    public bool HasNextPage => Page < TotalPages;

    public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}

/// <summary>
/// Factory for creating paged results.
/// </summary>
public static class PagedResult
{
    /// <summary>
    /// Creates an empty paged result with zero items.
    /// </summary>
    public static PagedResult<T> Empty<T>(int page, int pageSize)
        => new([], page, pageSize, 0);
}