using AuctionApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AuctionApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<LotStatus> LotStatuses => Set<LotStatus>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Lot> Lots => Set<Lot>();
    public DbSet<Bid> Bids => Set<Bid>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(e =>
        {
            e.HasIndex(r => r.Name).IsUnique();
            e.Property(r => r.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Username).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Username).HasMaxLength(100);
            e.Property(u => u.Email).HasMaxLength(255);
            e.Property(u => u.PasswordHash).HasMaxLength(255);
            e.HasOne(u => u.Role).WithMany(r => r.Users).HasForeignKey(u => u.RoleId);
        });

        modelBuilder.Entity<LotStatus>(e =>
        {
            e.HasIndex(s => s.Name).IsUnique();
            e.Property(s => s.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Tag>(e =>
        {
            e.HasIndex(t => t.Name).IsUnique();
            e.Property(t => t.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Lot>(e =>
        {
            e.Property(l => l.Title).HasMaxLength(255);
            e.Property(l => l.ImagePath).HasMaxLength(500);
            e.Property(l => l.StartingPrice).HasColumnType("numeric(12,2)");
            e.Property(l => l.CurrentPrice).HasColumnType("numeric(12,2)");

            e.HasOne(l => l.Seller)
                .WithMany(u => u.Lots)
                .HasForeignKey(l => l.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(l => l.Status)
                .WithMany(s => s.Lots)
                .HasForeignKey(l => l.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(l => l.WinnerBid)
                .WithMany()
                .HasForeignKey(l => l.WinnerBidId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            e.HasMany(l => l.Tags)
                .WithMany(t => t.Lots)
                .UsingEntity(j => j.ToTable("LotTags"));
        });

        modelBuilder.Entity<Bid>(e =>
        {
            e.Property(b => b.Amount).HasColumnType("numeric(12,2)");

            e.HasOne(b => b.Lot)
                .WithMany(l => l.Bids)
                .HasForeignKey(b => b.LotId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(b => b.Bidder)
                .WithMany(u => u.Bids)
                .HasForeignKey(b => b.BidderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin" },
            new Role { Id = 2, Name = "User" }
        );

        modelBuilder.Entity<LotStatus>().HasData(
            new LotStatus { Id = 1, Name = "Active" },
            new LotStatus { Id = 2, Name = "Sold" },
            new LotStatus { Id = 3, Name = "Cancelled" }
        );

        modelBuilder.Entity<Tag>().HasData(
            new Tag { Id = 1, Name = "Електроніка" },
            new Tag { Id = 2, Name = "Книги" },
            new Tag { Id = 3, Name = "Мистецтво" },
            new Tag { Id = 4, Name = "Одяг та взуття" },
            new Tag { Id = 5, Name = "Колекціонування" },
            new Tag { Id = 6, Name = "Транспорт" },
            new Tag { Id = 7, Name = "Меблі" },
            new Tag { Id = 8, Name = "Ювелірні вироби" },
            new Tag { Id = 9, Name = "Спорт" },
            new Tag { Id = 10, Name = "Інше" }
        );

        const string hash = "$2a$11$PLACEHOLDER_REPLACED_AT_RUNTIME_BY_DBINITIALIZER";

        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "admin", Email = "admin@auction.dev", PasswordHash = hash, RoleId = 1, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new User { Id = 2, Username = "mykola", Email = "mykola@example.com", PasswordHash = hash, RoleId = 2, CreatedAt = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new User { Id = 3, Username = "halyna", Email = "halyna@example.com", PasswordHash = hash, RoleId = 2, CreatedAt = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc) },
            new User { Id = 4, Username = "bohdan", Email = "bohdan@example.com", PasswordHash = hash, RoleId = 2, CreatedAt = new DateTime(2025, 1, 4, 0, 0, 0, DateTimeKind.Utc) },
            new User { Id = 5, Username = "oksana", Email = "oksana@example.com", PasswordHash = hash, RoleId = 2, CreatedAt = new DateTime(2025, 1, 5, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
