using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project02.Models;

[Table("Film")]
[Index("Film_Slug", Name = "IX_Film_Slug")]
[Index("Film_Name", Name = "UQ_Film_Name", IsUnique = true)]
[Index("Film_Slug", Name = "UQ_Film_Slug", IsUnique = true)]
public partial class Film
{
    [Key]
    public long Film_ID { get; set; }

    [StringLength(255)]
    public string Film_Slug { get; set; } = null!;

    [StringLength(255)]
    public string Film_Name { get; set; } = null!;

    [StringLength(100)]
    public string Film_Type { get; set; } = null!;

    public string Film_Description { get; set; } = null!;

    public short Film_Duration { get; set; }

    [StringLength(30)]
    public string Film_Status { get; set; } = null!;

    public DateTime Film_Created_At { get; set; }

    public DateTime Film_Update_At { get; set; }

    [InverseProperty("Film")]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [InverseProperty("Film")]
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    [ForeignKey("Film_ID")]
    [InverseProperty("Films")]
    public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();
}
