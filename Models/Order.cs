using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project02.Models;

public partial class Order
{
    [Key]
    public long Order_ID { get; set; }

    public long User_ID { get; set; }

    public long Showtime_ID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime OrderDate { get; set; }

    [Column(TypeName = "decimal(19, 0)")]
    public decimal TotalAmount { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Status { get; set; } = null!;

    [InverseProperty("Order")]
    public virtual ICollection<OrderSeat> OrderSeats { get; set; } = new List<OrderSeat>();

    [ForeignKey("Showtime_ID")]
    [InverseProperty("Orders")]
    public virtual Showtime Showtime { get; set; } = null!;

    [ForeignKey("User_ID")]
    [InverseProperty("Orders")]
    public virtual User User { get; set; } = null!;
}
