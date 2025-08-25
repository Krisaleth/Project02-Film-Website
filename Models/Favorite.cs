using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project02.Models;

[PrimaryKey("Person_ID", "Film_ID")]
public partial class Favorite
{
    [Key]
    public long Person_ID { get; set; }

    [Key]
    public long Film_ID { get; set; }

    public DateTime Created_At { get; set; }

    [ForeignKey("Film_ID")]
    [InverseProperty("Favorites")]
    public virtual Film Film { get; set; } = null!;

    [ForeignKey("Person_ID")]
    [InverseProperty("Favorites")]
    public virtual Person Person { get; set; } = null!;
}
