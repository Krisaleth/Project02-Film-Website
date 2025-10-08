
namespace Project02.ViewModels.Customer
{
    public class MovieShowVm
    {
        public string MovieSlug { get; set; }
        public string MoviePoster { get; set; }
        public string MovieName { get; set; }
        public int MovieYear { get; set; }
        public Project02.Models.Genre genre { get; set; }
    }
}
