using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project02.Models;

[PrimaryKey("Users_ID", "Movie_ID")]
public partial class Favorite
{
    [Key]
    public long Users_ID { get; set; }

    [Key]
    public long Movie_ID { get; set; }

    public DateTime Created_At { get; set; }

    [ForeignKey("Movie_ID")]
    [InverseProperty("Favorites")]
    public virtual Movie Movie { get; set; } = null!;

    [ForeignKey("Users_ID")]
    [InverseProperty("Favorites")]
    public virtual User Users { get; set; } = null!;
}
