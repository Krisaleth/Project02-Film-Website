namespace Project02.ViewModels.Customer
{
    public class SeatInfo
    {
        public long SeatId { get; set; }
        public string RowNumber { get; set; }
        public string SeatNumber { get; set; }
    }

    public class ConfirmOrderVm
    {
        public long ShowtimeId { get; set; }
        public int SelectedSeatsCount { get; set; }
        public decimal TotalPrice { get; set; }
        public List<SeatInfo> SelectedSeats { get; set; } = new List<SeatInfo>();
    }

}
