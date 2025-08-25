using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project02.Models;

[Table("Person")]
[Index("Person_Email", Name = "UQ_Person_Email", IsUnique = true)]
[Index("Person_FullName", Name = "UQ_Person_FullName", IsUnique = true)]
public partial class Person
{
    [Key]
    public long Person_ID { get; set; }

    [StringLength(255)]
    public string Person_FullName { get; set; } = null!;

    [StringLength(255)]
    public string Person_Email { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string Person_Phone { get; set; } = null!;

    public long? Account_ID { get; set; }

    [ForeignKey("Account_ID")]
    [InverseProperty("People")]
    public virtual Account? Account { get; set; }

    [InverseProperty("Person")]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [InverseProperty("Person")]
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}
