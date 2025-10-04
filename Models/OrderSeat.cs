using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project02.Models;

public partial class OrderSeat
{
    [Key]
    public long OrderSeat_ID { get; set; }

    public long Order_ID { get; set; }

    public long Seat_ID { get; set; }

    [Column(TypeName = "decimal(19, 0)")]
    public decimal Price { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Status { get; set; } = null!;

    [ForeignKey("Order_ID")]
    [InverseProperty("OrderSeats")]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey("Seat_ID")]
    [InverseProperty("OrderSeats")]
    public virtual Seat Seat { get; set; } = null!;

    [InverseProperty("OrderSeat")]
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
