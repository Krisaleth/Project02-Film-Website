namespace Project02.ViewModels.Movie
{
    public class MovieRowVm
    {
        public long Movie_ID { get; set; }
        public string Movie_Slug { get; set; } = default!;
        public string Movie_Name { get; set; } = default!;
        public string Movie_Description { get; set; } = default!;
        public short Movie_Duration { get; set; } = default!;
        public string Movie_Status { get; set; } = default!;
        public string Movie_Poster { get; set; } = default!;
    }

    public class MovieIndexVm
    {
        public List<MovieRowVm> Items { get; set; } = new();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public string? Q { get; set; }
        public int TotalPage => (int)Math.Ceiling((double)TotalItems / PageSize);
        public int Start => TotalItems == 0 ? 0 : (Page - 1) * PageSize + 1;
        public int End => Math.Min(Page *  PageSize, TotalItems);
    }
}
