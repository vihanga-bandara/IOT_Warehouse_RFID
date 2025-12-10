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
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<UserRight> UserRights { get; set; } = null!;
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

        // Role configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.RoleId);
        });

        // UserRight configuration (many-to-many junction table)
        modelBuilder.Entity<UserRight>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });
            
            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRights)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRights)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
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
                .HasPrincipalKey(s => s.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(t => t.Timestamp);
        });

        // Scanner configuration
        modelBuilder.Entity<Scanner>(entity =>
        {
            entity.HasIndex(e => e.DeviceId).IsUnique();
        });

        // Seed initial data
        DbSeeder.SeedData(modelBuilder);
    }
}
