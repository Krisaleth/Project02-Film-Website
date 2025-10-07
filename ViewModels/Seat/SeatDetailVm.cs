using System.Runtime.CompilerServices;

namespace Project02.ViewModels.Seat
{
    public class SeatDetailVm
    {
        public long Seat_ID { get; set; }
        public long Hall_ID { get; set; }
        public string Cinema_Name { get; set; } = default!;
        public string RowNumber { get; set; } = default!;
        public string SeatNumber { get; set; } = default!;
        public decimal Price { get; set; }
        public string Description { get; set; } = default!;
        public string SeatType { get; set; } = default!;
        public string Status { get; set; } = default!;

    }
}
