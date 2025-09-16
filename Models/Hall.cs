using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project02.Models;

public partial class Hall
{
    [Key]
    public long Hall_ID { get; set; }

    public long Cinema_ID { get; set; }

    public int Capacity { get; set; }

    [ForeignKey("Cinema_ID")]
    [InverseProperty("Halls")]
    public virtual Cinema Cinema { get; set; } = null!;

    [InverseProperty("Hall")]
    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

    [InverseProperty("Hall")]
    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}
