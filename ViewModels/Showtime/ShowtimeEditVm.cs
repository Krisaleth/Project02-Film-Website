using System.ComponentModel.DataAnnotations;

namespace Project02.ViewModels.Showtime
{
    public class ShowtimeEditVm
    {
        [Required]
        public long Showtime_ID { get; set; }
        [Required]
        public long Movie_ID { get; set; } = default!;
        [Required]
        public string Language { get; set; } = default!;
        [Required]
        public string Format { get; set; } = default!;
        [Required]
        public long Hall_ID { get; set; } = default!;
        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime StartTime { get; set; } = default!;
        public string StartTimeFormatted => StartTime.ToString("yyyy-MM-ddTHH:mm");
        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime EndTime { get; set; } = default!;
        public string EndTimeFormatted => EndTime.ToString("yyyy-MM-ddTHH:mm");
    }

}
