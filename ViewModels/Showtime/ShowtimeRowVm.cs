using Microsoft.AspNetCore.Mvc.Rendering;
using System.Numerics;

namespace Project02.ViewModels.Showtime
{
    public class ShowtimeRowVm
    {
        public long Showtime_ID { get; set; }
        public long Movie_ID { get; set; }
        public string MovieTitle { get; set; } = default!;
        public string CinemaName { get; set; } = default!;
        public long Hall_ID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class ShowtimeIndexVm
    {
        public List<ShowtimeRowVm> Showtimes { get; set; } = new List<ShowtimeRowVm>();
        public int Page { get; set; } = 1;
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? SearchString { get; set; }
        public string? SortOrder { get; set; }
        public IEnumerable<SelectListItem>? SortOptions { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public int EndPage => (CurrentPage - 1) * PageSize + Showtimes.Count;
        public int StartPage => (CurrentPage - 1) * PageSize + 1;
    }
}
