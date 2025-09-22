using Microsoft.AspNetCore.Mvc.Rendering;

namespace Project02.ViewModels.Cinema
{
    public class CinemaRowVm
    {
        public long Cinema_ID { get; set; }
        public string Cinema_Name { get; set; } = default!;
        public string Location { get; set; } = default!;
        public string Contact_Info { get; set; } = default!;
    }

    public class CinemaIndexVm
    {
        public List<CinemaRowVm> Items { get; set; } = new();
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
