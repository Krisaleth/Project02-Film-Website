using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project02.Models;

[Index("Cinema_Name", Name = "UQ__Cinemas__AE9CA7870946423D", IsUnique = true)]
public partial class Cinema
{
    [Key]
    public long Cinema_ID { get; set; }

    [StringLength(50)]
    public string Cinema_Name { get; set; } = null!;

    [StringLength(255)]
    public string Location { get; set; } = null!;

    [StringLength(200)]
    public string Contact_Info { get; set; } = null!;

    [InverseProperty("Cinema")]
    public virtual ICollection<Hall> Halls { get; set; } = new List<Hall>();

    [InverseProperty("Cinema")]
    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}
