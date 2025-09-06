using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project02.Models;

[Index("Users_Email", Name = "UQ_Users_Email", IsUnique = true)]
[Index("Users_FullName", Name = "UQ_Users_FullName", IsUnique = true)]
public partial class User
{
    [Key]
    public long Users_ID { get; set; }

    [StringLength(255)]
    public string Users_FullName { get; set; } = null!;

    [StringLength(255)]
    public string Users_Email { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string Users_Phone { get; set; } = null!;

    public byte[] RowsVersion { get; set; } = null!;

    public long? Account_ID { get; set; }

    [ForeignKey("Account_ID")]
    [InverseProperty("Users")]
    public virtual Account? Account { get; set; }

    [InverseProperty("Users")]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [InverseProperty("Users")]
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}
