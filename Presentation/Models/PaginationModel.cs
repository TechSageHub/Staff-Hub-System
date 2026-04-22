namespace Presentation.Models;

public class PaginationModel
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
    public int FirstItemNumber { get; set; }
    public int LastItemNumber { get; set; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)System.Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    /// <summary>
    /// Filter/search values that should be preserved across page links.
    /// </summary>
    public Dictionary<string, string?> RouteValues { get; set; } = new();
}
