using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project02.Models;

[Index("User_Email", Name = "UQ_Users_Email", IsUnique = true)]
[Index("User_Name", Name = "UQ_Users_FullName", IsUnique = true)]
public partial class User
{
    [Key]
    public long User_ID { get; set; }

    [StringLength(255)]
    public string User_Name { get; set; } = null!;

    [StringLength(255)]
    public string User_Email { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string User_Phone { get; set; } = null!;

    public byte[] RowsVersion { get; set; } = null!;

    public long? Account_ID { get; set; }

    [ForeignKey("Account_ID")]
    [InverseProperty("Users")]
    public virtual Account? Account { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
