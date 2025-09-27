namespace Project02.ViewModels.Showtime
{
    public class ShowtimeDetailsVm
    {
        public long Showtime_ID { get; set; }
        public long Movie_ID { get; set; }
        public string MovieTitle { get; set; } = default!;
        public string CinemaName { get; set; } = default!;
        public string Language { get; set; } = default!;
        public string Format { get; set; } = default!;
        public long Hall_ID { get; set; }
        public string HallName { get; set; } = default!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
