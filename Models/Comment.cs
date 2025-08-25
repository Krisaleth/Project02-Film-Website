using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project02.Models;

public partial class Comment
{
    [Key]
    public long Comment_ID { get; set; }

    public long Person_ID { get; set; }

    public long Film_ID { get; set; }

    public string Content { get; set; } = null!;

    public DateTime Created_At { get; set; }

    [ForeignKey("Film_ID")]
    [InverseProperty("Comments")]
    public virtual Film Film { get; set; } = null!;

    [ForeignKey("Person_ID")]
    [InverseProperty("Comments")]
    public virtual Person Person { get; set; } = null!;
}
