using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
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
        public string StartTimeFormatted => StartTime.ToString("dd/MM/yyyy h:mm tt", new CultureInfo("vi-VN"));
        public DateTime EndTime { get; set; }
        public string EndTimeFormatted => EndTime.ToString("dd/MM/yyyy h:mm tt", new CultureInfo("vi-VN"));
    }
    public class ShowtimeIndexVm
    {
        public List<ShowtimeRowVm> Showtimes { get; set; } = new List<ShowtimeRowVm>();
        public int Page { get; set; } = 1;  // Dùng Page để lưu trang hiện tại
        public int TotalPages { get; set; }
        public string? SearchString { get; set; }
        public string? SortOrder { get; set; }
        public IEnumerable<SelectListItem>? SortOptions { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public int EndPage => (Page - 1) * PageSize + Showtimes.Count;
        public int StartPage => (Page - 1) * PageSize + 1;
    }

}
