using Microsoft.AspNetCore.Mvc.Rendering;

namespace Project02.ViewModels.Ticket
{
    public class TicketRowVm
    {
        public long Ticket_ID { get; set; }
        public long Showtime_ID { get; set; }
        public string Movie_Title { get; set; } = default!;
        public DateTime Showtime { get; set; }
        public string Seat_Number { get; set; } = default!;
        public decimal Price { get; set; }
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
