using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Project02.Models;

namespace Project02.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Film> Films { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Person> People { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Account_ID).HasName("PK__Account__B19E45C96D5BDC4D");

            entity.Property(e => e.Create_At).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Password_Algo).HasDefaultValue("PBKDF2");
            entity.Property(e => e.Password_Iterations).HasDefaultValue(100000);
            entity.Property(e => e.Status).HasDefaultValue(true);
        });

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.Admin_ID).HasName("PK__Admin__4A300117D0196864");

            entity.HasOne(d => d.Account).WithMany(p => p.Admins)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Admin_Account");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Comment_ID).HasName("PK__Comments__99FC143BF0DEC7BC");

            entity.Property(e => e.Created_At).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Film).WithMany(p => p.Comments).HasConstraintName("FK_Comments_Film");

            entity.HasOne(d => d.Person).WithMany(p => p.Comments).HasConstraintName("FK_Comments_Person");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.Property(e => e.Created_At).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Film).WithMany(p => p.Favorites).HasConstraintName("FK_Favorites_Film");

            entity.HasOne(d => d.Person).WithMany(p => p.Favorites).HasConstraintName("FK_Favorites_Person");
        });

        modelBuilder.Entity<Film>(entity =>
        {
            entity.HasKey(e => e.Film_ID).HasName("PK__Film__CE6092FC5B809D15");

            entity.Property(e => e.Film_Created_At).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Film_Status).HasDefaultValue("Publish");
            entity.Property(e => e.Film_Update_At).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasMany(d => d.Genres).WithMany(p => p.Films)
                .UsingEntity<Dictionary<string, object>>(
                    "FilmGenre",
                    r => r.HasOne<Genre>().WithMany()
                        .HasForeignKey("Genres_ID")
                        .HasConstraintName("FK_FilmGenres_Genres"),
                    l => l.HasOne<Film>().WithMany()
                        .HasForeignKey("Film_ID")
                        .HasConstraintName("FK_FilmGenres_Film"),
                    j =>
                    {
                        j.HasKey("Film_ID", "Genres_ID");
                        j.ToTable("FilmGenres");
                    });
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.Genres_ID).HasName("PK__Genres__2232D7A631FE823D");
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.Person_ID).HasName("PK__Person__7EABD08B3F0254D9");

            entity.HasOne(d => d.Account).WithMany(p => p.People)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Person_Account");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
