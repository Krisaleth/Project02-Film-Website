using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project02.Models;

[Table("Admin")]
[Index("Admin_Email", Name = "UQ_Admin_Email", IsUnique = true)]
[Index("Admin_FullName", Name = "UQ_Admin_FullName", IsUnique = true)]
public partial class Admin
{
    [Key]
    public long Admin_ID { get; set; }

    [StringLength(255)]
    public string Admin_FullName { get; set; } = null!;

    [StringLength(255)]
    public string Admin_Email { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string Admin_Phone { get; set; } = null!;

    public long? Account_ID { get; set; }

    [ForeignKey("Account_ID")]
    [InverseProperty("Admins")]
    public virtual Account? Account { get; set; }
}
