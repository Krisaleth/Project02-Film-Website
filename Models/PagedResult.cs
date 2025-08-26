namespace Project02.Models
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = [];
        public int PageIndex { get; init; }
        public int PageSize { get; init; }  
        public int TotalItems { get; init; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
}
