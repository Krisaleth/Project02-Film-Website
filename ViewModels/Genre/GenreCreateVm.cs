using System.ComponentModel.DataAnnotations;

namespace Project02.ViewModels.Genre
{
    public class GenreCreateVm
    {
        [Required]
        public string Genre_Name { get; set; } = default!;
        
    }
}
