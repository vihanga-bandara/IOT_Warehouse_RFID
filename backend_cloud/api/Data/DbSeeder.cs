using Microsoft.EntityFrameworkCore;
using RfidWarehouseApi.Models;

namespace RfidWarehouseApi.Data;

public static class DbSeeder
{
    public static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Roles first
        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                RoleId = 1,
                RoleName = "Admin",
                Description = "Administrator with full access"
            },
            new Role
            {
                RoleId = 2,
                RoleName = "User",
                Description = "Regular user with limited access"
            }
        );

        // Seed Scanners
        modelBuilder.Entity<Scanner>().HasData(
            new Scanner
            {
                ScannerId = 1,
                DeviceId = "rpi-scanner-01",
                Name = "Main Warehouse Entrance",
                Status = "active"
            }
        );

        // Seed Users (password: "password123")
        var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        var userPasswordHash = BCrypt.Net.BCrypt.HashPassword("password123");

        modelBuilder.Entity<User>().HasData(
            new User
            {
                UserId = 1,
                Email = "admin@warehouse.com",
                PasswordHash = adminPasswordHash,
                Name = "Admin",
                Lastname = "User"
            },
            new User
            {
                UserId = 2,
                Email = "john.doe@warehouse.com",
                PasswordHash = userPasswordHash,
                Name = "John",
                Lastname = "Doe"
            }
        );

        // Seed UserRights (assign roles to users)
        modelBuilder.Entity<UserRight>().HasData(
            new UserRight
            {
                UserId = 1,
                RoleId = 1 // Admin user has Admin role
            },
            new UserRight
            {
                UserId = 2,
                RoleId = 2 // John has User role
            }
        );

        // Seed Items
        modelBuilder.Entity<Item>().HasData(
            new Item
            {
                ItemId = 1,
                RfidUid = "RFID001234567890",
                ItemName = "Power Drill",
                Status = ItemStatus.Available,
                LastUpdated = DateTime.UtcNow
            },
            new Item
            {
                ItemId = 2,
                RfidUid = "RFID001234567891",
                ItemName = "Hammer",
                Status = ItemStatus.Available,
                LastUpdated = DateTime.UtcNow
            },
            new Item
            {
                ItemId = 3,
                RfidUid = "RFID001234567892",
                ItemName = "Screwdriver Set",
                Status = ItemStatus.Available,
                LastUpdated = DateTime.UtcNow
            },
            new Item
            {
                ItemId = 4,
                RfidUid = "RFID001234567893",
                ItemName = "Measuring Tape",
                Status = ItemStatus.Available,
                LastUpdated = DateTime.UtcNow
            },
            new Item
            {
                ItemId = 5,
                RfidUid = "RFID001234567894",
                ItemName = "Wrench Set",
                Status = ItemStatus.Available,
                LastUpdated = DateTime.UtcNow
            }
        );
    }
}
