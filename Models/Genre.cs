
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project02.Models;

[Index("Genre_Slug", Name = "IX_Genres_Slug")]
[Index("Genre_Slug", Name = "UQ_Genre_Slug", IsUnique = true)]
public partial class Genre
{
    [Key]
    public long Genre_ID { get; set; }

    [StringLength(100)]
    public string Genre_Name { get; set; } = null!;

    [StringLength(100)]
    public string Genre_Slug { get; set; } = null!;

    [ForeignKey("Genre_ID")]
    [InverseProperty("Genres")]
    public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();
}
