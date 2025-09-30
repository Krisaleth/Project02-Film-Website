using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Project02.ViewModels.Order
{
    public class OrderCreateVm
    {
        [Required]
        public long User_ID { get; set; } = 0;
        [Required]
        public DateTime Order_Date { get; set; }
        public string Order_Date_Formatted => Order_Date.ToString("dd/MM/yyyy h:mm tt", new CultureInfo("vi-VN"));

        public List<SeatSelectionVm> SelectedSeat = new List<SeatSelectionVm>();
    }

    public class SeatSelectionVm
    {
        public long Seat_ID { get; set; }
        public string Seat_Type { get; set; } = null!;
        public int Quantity { get; set; } = 1; // Nếu ghế chỉ chọn 1, quantity luôn 1
        public decimal Price { get; set; }
    }
}
