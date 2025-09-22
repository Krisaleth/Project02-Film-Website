using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project02.Models;

public partial class Seat
{
    [Key]
    public long Seat_ID { get; set; }

    public long Hall_ID { get; set; }

    [StringLength(1)]
    public string RowNumber { get; set; } = null!;

    [StringLength(2)]
    public string SeatNumber { get; set; } = null!;

    [StringLength(10)]
    public string SeatType { get; set; } = null!;

    [ForeignKey("Hall_ID")]
    [InverseProperty("Seats")]
    public virtual Hall Hall { get; set; } = null!;

    [InverseProperty("Seat")]
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
