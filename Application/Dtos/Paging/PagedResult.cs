namespace Application.Dtos.Paging;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int TotalCount { get; init; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
    public int FirstItemNumber => TotalCount == 0 ? 0 : ((Page - 1) * PageSize) + 1;
    public int LastItemNumber => Math.Min(Page * PageSize, TotalCount);
}

public class PageRequest
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

    private int _pageSize = DefaultPageSize;
    private int _page = 1;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value <= 0 ? DefaultPageSize : Math.Min(value, MaxPageSize);
    }

    public int Skip => (Page - 1) * PageSize;
}
