namespace Project02.ViewModels.Hall
{
    public class HallRowVm
    {
        public long Hall_ID { get; set; }
        public string Cinema_Name { get; set; } = default!;
        public int Capacity { get; set; }
    }

    public class HallIndexVm
    {
        public List<HallRowVm> Items { get; set; } = new();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public string? Q { get; set; }
        public string? sortOrder { get; set; }
        public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>? SortOptions { get; set; }
        public int TotalPage => (int)Math.Ceiling((double)TotalItems / PageSize);
        public int Start => TotalItems == 0 ? 0 : (Page - 1) * PageSize + 1;
        public int End => Math.Min(Page * PageSize, TotalItems);
    }
}
