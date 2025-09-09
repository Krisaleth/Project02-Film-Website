using System.ComponentModel.DataAnnotations;

namespace Project02.ViewModels.Movie
{
    public class MovieCreateVm
    {
        [Required]
        public string Movie_Name { get; set; } = default!;
        [Required]
        public string Movie_Description { get; set; } = default!;
        [Required]
        public short Movie_Duration { get; set; }
        [Required]
        public string Movie_Status { get; set; } = "Publish";
        [Required(ErrorMessage = "Vui lòng chọn poster")]
        public IFormFile Movie_Poster { get; set; } = default!;

    }
}
