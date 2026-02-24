namespace PadTime.Application.Common.Models;

/// <summary>
/// Generic paginated result wrapper.
/// </summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
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
    public static PagedResult<T> Empty<T>(int page, int pageSize)
        => new([], page, pageSize, 0);
}
