using System.ComponentModel.DataAnnotations;

namespace Project02.ViewModels.Movie
{
    public class MovieEditVm
    {
        public long Movie_ID { get; set; }
        public string Movie_Slug { get; set; } = default!;
        [Required, StringLength(255)]
        public string Movie_Name { get; set; } = default!;
        [Required]
        public int Movie_Year { get; set; } = default!;
        [Required, StringLength(255)]
        public string Movie_Producer { get; set; } = default!;
        [Required]
        public string Movie_Description { get; set; } = default!;
        [Range(1, short.MaxValue, ErrorMessage = "Duration phải > 0")]
        public short Movie_Duration { get; set; }
        [Required, RegularExpression("^(Publish|Unpublish)$", ErrorMessage = "Status phải là Publish hoặc Unpublish")]
        public string Movie_Status { get; set; } = default!;
        public IFormFile? Movie_Poster { get; set; }
        public string? ExistingPoster { get; set; } = default!;
        [Required(ErrorMessage = "Thiếu RowsVersion, vui lỏng tải lại trang")]
        public byte[]? RowsVersion { get; set; }
    }
}
