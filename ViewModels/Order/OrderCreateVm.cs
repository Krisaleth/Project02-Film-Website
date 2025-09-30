using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Project02.ViewModels.Order;

public class OrderCreateVm
{
    [Required]
    public long User_ID { get; set; }

    [Required]
    public long Showtime_ID { get; set; }

    [Required]
    public string SelectedSeats { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal TotalAmount { get; set; }
}
