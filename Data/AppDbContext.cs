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

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Movie> Movies { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Account_ID).HasName("PK__Account__B19E45C9336E82CF");

            entity.Property(e => e.Create_At).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Password_Algo).HasDefaultValue("PBKDF2");
            entity.Property(e => e.Password_Iterations).HasDefaultValue(100000);
            entity.Property(e => e.Status).HasDefaultValue(true);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Comment_ID).HasName("PK__Comments__99FC143B18FFA8C5");

            entity.Property(e => e.Created_At).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Movie).WithMany(p => p.Comments).HasConstraintName("FK_Comments_Movie");

            entity.HasOne(d => d.Users).WithMany(p => p.Comments).HasConstraintName("FK_Comments_Users");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.Property(e => e.Created_At).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Movie).WithMany(p => p.Favorites).HasConstraintName("FK_Favorites_Movie");

            entity.HasOne(d => d.Users).WithMany(p => p.Favorites).HasConstraintName("FK_Favorites_Users");
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.Genres_ID).HasName("PK__Genres__2232D7A64F02941E");
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.Movie_ID).HasName("PK__Movie__7A8804050BBE30EA");

            entity.Property(e => e.Movie_Created_At).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Movie_Status).HasDefaultValue("Publish");
            entity.Property(e => e.Movie_Update_At).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasMany(d => d.Genres).WithMany(p => p.Movies)
                .UsingEntity<Dictionary<string, object>>(
                    "MovieGenre",
                    r => r.HasOne<Genre>().WithMany()
                        .HasForeignKey("Genres_ID")
                        .HasConstraintName("FK_MovieGenres_Genres"),
                    l => l.HasOne<Movie>().WithMany()
                        .HasForeignKey("Movie_ID")
                        .HasConstraintName("FK_MovieGenres_Movie"),
                    j =>
                    {
                        j.HasKey("Movie_ID", "Genres_ID");
                        j.ToTable("MovieGenres");
                    });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Users_ID).HasName("PK__Users__EB68290DDD1BEFF7");

            entity.HasOne(d => d.Account).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Users_Account");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
