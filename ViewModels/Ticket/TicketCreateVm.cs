using System.ComponentModel.DataAnnotations;

namespace Project02.ViewModels.Ticket
{
    public class TicketCreateVm
    {
        [Required]
        public string Movie_Name { get; set; } = default!;
        [Required]
        public DateTime Start_Time { get; set; }
        [Required]
        public string SeatNumber { get; set; } = default!;
        [Required]
        public string User_Name { get; set; } = default!;
        [Required]
        public string User_Phone { get; set; } = default!;
        [Required, EmailAddress]
        public string User_Email { get; set; } = default!;
        [Required, Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        [Required]
        public string Payment_Method { get; set; } = default!;
        [Required]
        public string Payment_Status { get; set; } = default!;
        [Required]
        public string Status { get; set; } = default!;
    }
}
