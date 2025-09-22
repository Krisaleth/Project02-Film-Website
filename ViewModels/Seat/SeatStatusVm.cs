namespace Project02.ViewModels.Seat
{
    public class SeatStatusVm
    {
        public long Seat_ID { get; set; }
        public long Hall_ID { get; set; }
        public string Cinema_Name { get; set; } = default!;
        public string RowNumber { get; set; } = default!;
        public string SeatNumber { get; set; } = default!;
        public string SeatType { get; set; } = default!;
        public string Status { get; set; } = default!;
    }

    public class SeatIndexVm
    {
        public List<SeatStatusVm> Items { get; set; } = new();
        public long? selectedHallId { get; set; }
        public IEnumerable<string>? hallId { get; set; }
    }
}
