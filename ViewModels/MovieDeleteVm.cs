namespace Project02.ViewModels
{
    public class MovieDeleteVm
    {
        public long Movie_ID { get; set; }
        public string Movie_Name { get; set; } = default!;
        public string Movie_Poster { get; set; } = default!;
        public byte[]? RowVersion { get; set; }
    }
}
