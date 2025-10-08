namespace Project02.ViewModels.Customer
{
    public class MovieListVm
    {
        public IEnumerable<Project02.Models.Movie> Movies { get; set; }
        public IEnumerable<Project02.Models.Genre> Genres { get; set; }
        public string? SelectedGenreSlug { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalItems { get; set; }
        public string? SearchKeyword { get; set; }
        public int TotalPages
        {
            get
            {
                return (int)Math.Ceiling((double)TotalItems / PageSize);
            }
        }
    }
}
