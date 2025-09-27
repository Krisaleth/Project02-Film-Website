using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project02.Models;

public partial class Showtime
{
    [Key]
    public long Showtime_ID { get; set; }

    public long Movie_ID { get; set; }

    public long Hall_ID { get; set; }

    public DateTime Start_Time { get; set; }

    public DateTime End_Time { get; set; }

    [StringLength(20)]
    public string Language { get; set; } = null!;

    [StringLength(20)]
    public string Format { get; set; } = null!;

    [ForeignKey("Hall_ID")]
    [InverseProperty("Showtimes")]
    public virtual Hall Hall { get; set; } = null!;

    [ForeignKey("Movie_ID")]
    [InverseProperty("Showtimes")]
    public virtual Movie Movie { get; set; } = null!;
}
