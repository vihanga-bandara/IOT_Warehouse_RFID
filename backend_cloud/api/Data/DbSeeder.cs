using Microsoft.EntityFrameworkCore;
using RfidWarehouseApi.Models;

namespace RfidWarehouseApi.Data;

public static class DbSeeder
{
    public static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Scanners
        modelBuilder.Entity<Scanner>().HasData(
            new Scanner
            {
                ScannerId = 1,
                DeviceIdString = "rpi-scanner-01",
                LocationName = "Main Warehouse Entrance"
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
                Lastname = "User",
                RoleId = 1 // Admin
            },
            new User
            {
                UserId = 2,
                Email = "john.doe@warehouse.com",
                PasswordHash = userPasswordHash,
                Name = "John",
                Lastname = "Doe",
                RoleId = 2 // Regular user
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
