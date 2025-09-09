using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project02.Models;

[Table("Account")]
[Index("UserName", Name = "UQ_Account_UserName", IsUnique = true)]
public partial class Account
{
    [Key]
    public long Account_ID { get; set; }

    [StringLength(255)]
    public string UserName { get; set; } = null!;

    [MaxLength(512)]
    public byte[] Password_Hash { get; set; } = null!;

    [MaxLength(128)]
    public byte[] Password_Salt { get; set; } = null!;

    [StringLength(20)]
    public string Password_Algo { get; set; } = null!;

    public int Password_Iterations { get; set; }

    [StringLength(10)]
    public string Role { get; set; } = null!;

    [StringLength(10)]
    public string Status { get; set; } = null!;

    public DateTime Create_At { get; set; }

    [InverseProperty("Account")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
