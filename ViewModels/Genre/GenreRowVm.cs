using Microsoft.AspNetCore.Mvc.Rendering;

namespace Project02.ViewModels.Genre
{
    public class GenreRowVm
    {
        public long Genre_ID { get; set; }
        public string Genre_Name { get; set; } = default!;
        public string Genre_Description { get; set; } = default!;
        public string Genre_Slug { get; set; } = default!;
    }

    public class GenreIndexVm
    {
        public List<GenreRowVm> Items { get; set; } = new();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public string? search { get; set; }
        public string? sortOrder { get; set; }
        public IEnumerable<SelectListItem>? SortOptions { get; set; }
        public int TotalPage => (int)Math.Ceiling((double)TotalItems / PageSize);
        public int Start => TotalItems == 0 ? 0 : (Page - 1) * PageSize + 1;
        public int End => Math.Min(Page * PageSize, TotalItems);
    }
}
