using System.ComponentModel.DataAnnotations;

namespace Project02.ViewModels.Showtime
{
    public class ShowtimeCreateVm
    {
        [Required]
        public long Movie_ID { get; set; }
        [Required]
        public string Language { get; set; } = default!;
        [Required]
        public string Format { get; set; } = default!;
        [Required]
        public long Hall_ID { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
    }
}
