using System.ComponentModel.DataAnnotations;

namespace Project02.ViewModels.Showtime
{
    public class ShowtimeCreateVm
    {
        public ShowtimeCreateVm()
        {
            StartTime = new DateTime(2000, 1, 1, 0, 0, 0);
            EndTime = StartTime.AddHours(1);
        }
        [Required]
        public long Movie_ID { get; set; } = 0;
        [Required]
        public string Language { get; set; } = default!;
        [Required]
        public string Format { get; set; } = default!;
        [Required]
        public long Hall_ID { get; set; } = 0;
        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime StartTime { get; set; }
        public string StartTimeFormatted => StartTime.ToString("yyyy-MM-ddTHH:mm");

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime EndTime { get; set; }
        public string EndTimeFormatted => EndTime.ToString("yyyy-MM-ddTHH:mm");
    }
}
