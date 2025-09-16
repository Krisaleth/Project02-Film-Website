using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project02.Models;

public partial class Ticket
{
    [Key]
    public long Ticket_ID { get; set; }

    public long Showtime_ID { get; set; }

    public long Seat_ID { get; set; }

    public long User_ID { get; set; }

    [Column(TypeName = "decimal(19, 0)")]
    public decimal Price { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = null!;

    public DateTime BookingTime { get; set; }

    [InverseProperty("Ticket")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [ForeignKey("Seat_ID")]
    [InverseProperty("Tickets")]
    public virtual Seat Seat { get; set; } = null!;

    [ForeignKey("Showtime_ID")]
    [InverseProperty("Tickets")]
    public virtual Showtime Showtime { get; set; } = null!;

    [ForeignKey("User_ID")]
    [InverseProperty("Tickets")]
    public virtual User User { get; set; } = null!;
}
