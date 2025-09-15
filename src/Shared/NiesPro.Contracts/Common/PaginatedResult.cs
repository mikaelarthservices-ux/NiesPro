namespace NiesPro.Contracts.Common;

public class PaginatedResult<T>
{
    public IEnumerable<T> Items { get; }
    public int TotalCount { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public PaginatedResult(IEnumerable<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
    }

    public static PaginatedResult<T> Create(IEnumerable<T> items, int totalCount, int page, int pageSize)
    {
        return new PaginatedResult<T>(items, totalCount, page, pageSize);
    }

    public static PaginatedResult<T> Empty(int page, int pageSize)
    {
        return new PaginatedResult<T>(Enumerable.Empty<T>(), 0, page, pageSize);
    }
}