using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project02.Models;

[Table("Movie")]
[Index("Movie_Slug", Name = "IX_Movie_Slug")]
[Index("Movie_Name", Name = "UQ_Movie_Name", IsUnique = true)]
[Index("Movie_Slug", Name = "UQ_Movie_Slug", IsUnique = true)]
public partial class Movie
{
    [Key]
    public long Movie_ID { get; set; }

    [StringLength(255)]
    public string Movie_Slug { get; set; } = null!;

    [StringLength(255)]
    public string Movie_Name { get; set; } = null!;

    [StringLength(500)]
    public string Movie_Poster { get; set; } = null!;

    public string Movie_Description { get; set; } = null!;

    public short Movie_Duration { get; set; }

    public byte[] RowsVersion { get; set; } = null!;

    [StringLength(30)]
    public string Movie_Status { get; set; } = null!;

    public DateTime Movie_Created_At { get; set; }

    public DateTime Movie_Update_At { get; set; }

    [InverseProperty("Movie")]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [InverseProperty("Movie")]
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    [ForeignKey("Movie_ID")]
    [InverseProperty("Movies")]
    public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();
}
