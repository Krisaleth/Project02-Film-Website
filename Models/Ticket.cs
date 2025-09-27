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

    public long OrderSeat_ID { get; set; }

    public long Showtime_ID { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = null!;

    public DateTime BookingTime { get; set; }

    [ForeignKey("OrderSeat_ID")]
    [InverseProperty("Tickets")]
    public virtual OrderSeat OrderSeat { get; set; } = null!;
}
