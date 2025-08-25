using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project02.Models;

[Index("Genres_Slug", Name = "IX_Genres_Slug")]
[Index("Genres_Slug", Name = "UQ_Genres_Slug", IsUnique = true)]
public partial class Genre
{
    [Key]
    public long Genres_ID { get; set; }

    [StringLength(100)]
    public string Genres_Name { get; set; } = null!;

    [StringLength(100)]
    public string Genres_Slug { get; set; } = null!;

    [ForeignKey("Genres_ID")]
    [InverseProperty("Genres")]
    public virtual ICollection<Film> Films { get; set; } = new List<Film>();
}
