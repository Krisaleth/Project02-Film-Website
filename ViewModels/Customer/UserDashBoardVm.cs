namespace Project02.ViewModels.Customer
{
    public class UserDashBoardVm
    {
        public List<MovieShowVm> Movies { get; set; } = new();
        public List<MovieShowVm> RandomMovies { get; set; } = new();
        public List<string> MoviesWithShowtime { get; set; }

    }
}
