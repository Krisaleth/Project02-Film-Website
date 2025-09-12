using Project02.ViewModels.Genre;
using Project02.ViewModels.Movie;

namespace Project02.ViewModels.MovieDetailVm;

public class MovieDetailVm
{
    public MovieRowVm Movie { get; set; } = default!;
    public List<GenreRowVm> RemainingGenres { get; set; } = new List<GenreRowVm>();
    public List<GenreRowVm> Genres { get; set; } = new List<GenreRowVm>();
    public string NewGenreName { get; set; } = string.Empty;
}
