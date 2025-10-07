using Microsoft.AspNetCore.Mvc.Rendering;
using Project02.Models;

namespace Project02.ViewModels.Ticket
{
    public class TicketRowVm
    {
        public long Ticket_ID { get; set; }
        public long OrderSeat_ID { get; set; }
        public long Showtime_ID { get; set; }
        public string Movie_Name { get; set; } = default!;
        public DateTime Start_Time { get; set; }
        public string SeatNumber { get; set; } = default!;
        public DateTime BookingTime { get; set; }
        public DateTime ShowtimeStartTime { get; set; }
        public string Status { get; set; } = default!;

    }

    public class TicketIndexVm
    {
        public List<TicketRowVm> Items { get; set; } = new();
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
