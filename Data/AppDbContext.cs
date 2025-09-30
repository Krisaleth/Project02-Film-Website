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

    public virtual DbSet<Cinema> Cinemas { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Hall> Halls { get; set; }

    public virtual DbSet<Movie> Movies { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderSeat> OrderSeats { get; set; }

    public virtual DbSet<Seat> Seats { get; set; }

    public virtual DbSet<Showtime> Showtimes { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Account_ID).HasName("PK__Account__B19E45C9A7DA3B9A");

            entity.Property(e => e.Create_At).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Password_Algo).HasDefaultValue("PBKDF2");
            entity.Property(e => e.Password_Iterations).HasDefaultValue(100000);
            entity.Property(e => e.Status).HasDefaultValue("Active");
        });

        modelBuilder.Entity<Cinema>(entity =>
        {
            entity.HasKey(e => e.Cinema_ID).HasName("PK__Cinemas__89C6DAE1614C700D");
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.Genre_ID).HasName("PK__Genres__964A2006356F6D84");
        });

        modelBuilder.Entity<Hall>(entity =>
        {
            entity.HasKey(e => e.Hall_ID).HasName("PK__Halls__927F7126F015F2AC");

            entity.HasOne(d => d.Cinema).WithMany(p => p.Halls)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Halls_Cinema_ID");
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.Movie_ID).HasName("PK__Movies__7A880405C3FC6043");

            entity.Property(e => e.Movie_Created_At).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Movie_Producer).HasDefaultValue("Unknown");
            entity.Property(e => e.Movie_Status).HasDefaultValue("Publish");
            entity.Property(e => e.Movie_Update_At).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Movie_Year).HasDefaultValue(2000);
            entity.Property(e => e.RowsVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasMany(d => d.Genres).WithMany(p => p.Movies)
                .UsingEntity<Dictionary<string, object>>(
                    "MovieGenre",
                    r => r.HasOne<Genre>().WithMany()
                        .HasForeignKey("Genre_ID")
                        .HasConstraintName("FK_MovieGenre_Genre"),
                    l => l.HasOne<Movie>().WithMany()
                        .HasForeignKey("Movie_ID")
                        .HasConstraintName("FK_MovieGenre_Movie"),
                    j =>
                    {
                        j.HasKey("Movie_ID", "Genre_ID").HasName("PK_MovieGenre");
                        j.ToTable("MovieGenres");
                    });
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Order_ID).HasName("PK__Orders__F1E4639BD776D77E");

            entity.Property(e => e.Status).HasDefaultValue("Pending");

            entity.HasOne(d => d.Showtime).WithMany(p => p.Orders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Showtime");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_User");
        });

        modelBuilder.Entity<OrderSeat>(entity =>
        {
            entity.HasKey(e => e.OrderSeat_ID).HasName("PK__OrderSea__5DF03B34BF918713");

            entity.Property(e => e.Status).HasDefaultValue("Booked");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderSeats)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderSeats_Order");

            entity.HasOne(d => d.Seat).WithMany(p => p.OrderSeats)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderSeats_Seat");
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(e => e.Seat_ID).HasName("PK__Seats__8B2CE7B6BAD901D0");

            entity.HasOne(d => d.Hall).WithMany(p => p.Seats)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Seats_Hall_ID");
        });

        modelBuilder.Entity<Showtime>(entity =>
        {
            entity.HasKey(e => e.Showtime_ID).HasName("PK__Showtime__7C7A90896D09888F");

            entity.HasOne(d => d.Hall).WithMany(p => p.Showtimes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Showtimes_Hall_ID");

            entity.HasOne(d => d.Movie).WithMany(p => p.Showtimes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Showtimes_Movie_ID");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Ticket_ID).HasName("PK__Tickets__ED7260D9A515EDFA");

            entity.Property(e => e.Status).HasDefaultValue("Available");

            entity.HasOne(d => d.OrderSeat).WithMany(p => p.Tickets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tickets_OrderSeat");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.User_ID).HasName("PK__Users__206D919031E7A3D2");

            entity.Property(e => e.RowsVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Account).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Users_Account");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
