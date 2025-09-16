using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project02.Models;

public partial class Payment
{
    [Key]
    public long Payment_ID { get; set; }

    public long User_ID { get; set; }

    public long Ticket_ID { get; set; }

    public int Amount { get; set; }

    [StringLength(50)]
    public string PaymentMethod { get; set; } = null!;

    [StringLength(50)]
    public string PaymentStatus { get; set; } = null!;

    public DateTime PaymentTime { get; set; }

    [ForeignKey("Ticket_ID")]
    [InverseProperty("Payments")]
    public virtual Ticket Ticket { get; set; } = null!;

    [ForeignKey("User_ID")]
    [InverseProperty("Payments")]
    public virtual User User { get; set; } = null!;
}
