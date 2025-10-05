namespace Orders.Application.DTOs
{
    /// <summary>
    /// Generic wrapper for paginated results.
    /// </summary>
    public sealed record PagedResult<T>(
        IReadOnlyList<T> Items,
        int Page,
        int PageSize,
        int TotalCount
    );
}
