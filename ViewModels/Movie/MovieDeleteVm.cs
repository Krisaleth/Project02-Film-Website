namespace Project02.ViewModels.Movie
{
    public class MovieDeleteVm
    {
        public long Movie_ID { get; set; }

        public string Movie_Slug { get; set; } = default!;
        public string Movie_Name { get; set; } = default!;
        public string Movie_Poster { get; set; } = default!;
        public byte[]? RowsVersion { get; set; }
    }
}
