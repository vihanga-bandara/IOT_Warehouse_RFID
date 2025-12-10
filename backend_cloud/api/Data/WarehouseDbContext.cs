using Microsoft.EntityFrameworkCore;
using RfidWarehouseApi.Models;

namespace RfidWarehouseApi.Data;

public class WarehouseDbContext : DbContext
{
    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Item> Items { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<Scanner> Scanners { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.RfidTagUid);
        });

        // Item configuration
        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasIndex(e => e.RfidUid).IsUnique();
            
            entity.HasOne(i => i.CurrentHolder)
                .WithMany(u => u.Items)
                .HasForeignKey(i => i.CurrentHolderId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Transaction configuration
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Item)
                .WithMany(i => i.Transactions)
                .HasForeignKey(t => t.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Scanner)
                .WithMany(s => s.Transactions)
                .HasForeignKey(t => t.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(t => t.Timestamp);
        });

        // Scanner configuration
        modelBuilder.Entity<Scanner>(entity =>
        {
            entity.HasIndex(e => e.DeviceIdString).IsUnique();
        });

        // Seed initial data
        DbSeeder.SeedData(modelBuilder);
    }
}
