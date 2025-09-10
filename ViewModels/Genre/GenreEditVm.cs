using System.ComponentModel.DataAnnotations;

namespace Project02.ViewModels.Genre
{
    public class GenreEditVm
    {
        [Required]
        public long Genre_ID { get; set; }
        [Required]
        [StringLength(100)]
        public string Genre_Name { get; set; } = null!;
        public string Genre_Slug { get; set; } = null!;

    }
}
