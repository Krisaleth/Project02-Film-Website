using System.ComponentModel.DataAnnotations;

namespace Project02.ViewModels
{
    public class MovieEditVm
    {
        public long Movie_ID { get; set; }
        [Required]
        public string Movie_Name { get; set; } = default!;
        [Required]
        public string Movie_Description { get; set; } = default!;

        [Required]
        public short Movie_Duration { get; set; }
        [Required]
        public string Movie_Status { get; set; } = default!;
        public IFormFile? Movie_Poster { get; set; }
        public string ExistingPoster { get; set; } = default!;
        public byte[]? RowsVersion { get; set; }
    }
}
