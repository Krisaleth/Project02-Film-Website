using System.Globalization;

namespace Project02.ViewModels.Order
{
    public class OrderDetailVm
    {
        public long Order_ID { get; set; }
        public long User_ID { get; set; }
        public string UserName { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public string OrderDateString => OrderDate.ToString("dd/MM/yyyy h:mm tt", new CultureInfo("vi-VN"));
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public List<OrderSeatVm> OrderSeats { get; set; } = new List<OrderSeatVm>();
    }

    public class OrderSeatVm
    {
        public long OrderSeat_ID { get; set;}
        public long Seat_ID { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = null!;
    }
}
