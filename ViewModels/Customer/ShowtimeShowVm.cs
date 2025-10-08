namespace Project02.ViewModels.Customer
{
    public class ShowtimeShowVm
    {
        public List<Project02.Models.Showtime> Showtimes { get; set; }
        public Dictionary<long, List<Project02.Models.Hall>> HallsGroupedByCinema { get; set; }
        public string MovieName { get; set; }   
    }
}
