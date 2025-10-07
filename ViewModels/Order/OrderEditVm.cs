using System.ComponentModel.DataAnnotations;
using Project02.Models;

namespace Project02.ViewModels.Order
{
    public class OrderEditVm
    {
        [Required]
        public long Order_ID { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn User")]
        public long User_ID { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn suất chiếu")]
        public long Showtime_ID { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ghế")]
        public string SelectedSeats { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tổng tiền phải lớn hơn 0")]
        public decimal TotalAmount { get; set; }

        public List<long> SelectedSeatIds { get; set; } = new List<long>();
        public Models.Showtime? Showtime { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập trạng thái")]
        [StringLength(20, ErrorMessage = "Trạng thái không quá 20 ký tự")]
        public string Status { get; set; }

        
    }
}
