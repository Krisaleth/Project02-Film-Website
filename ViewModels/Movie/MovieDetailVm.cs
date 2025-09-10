namespace Project02.ViewModels.Movie
{
    public class MovieDetailVm
    {
        public string Movie_Name { get; set; } = default!;
        public string Movie_Slug { get; set; } = default!;
        public int Movie_Year { get; set; } = default!;
        public string Movie_Producer { get; set; } = default!;
        public string Movie_Description { get; set; } = default!;
        public string Movie_Status {  get; set; } = default!;
        public string Movie_Poster { get; set; } = default!;
        public string DurationFormatted { get; set; } = default!;
        public List<string> Genres { get; set; } = new();
    }
}
