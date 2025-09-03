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

    public long Users_ID { get; set; }

    public long Movie_ID { get; set; }

    public string Content { get; set; } = null!;

    public DateTime Created_At { get; set; }

    [ForeignKey("Movie_ID")]
    [InverseProperty("Comments")]
    public virtual Movie Movie { get; set; } = null!;

    [ForeignKey("Users_ID")]
    [InverseProperty("Comments")]
    public virtual User Users { get; set; } = null!;
}
