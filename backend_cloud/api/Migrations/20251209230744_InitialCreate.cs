using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RfidWarehouseApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Scanners",
                columns: table => new
                {
                    scanner_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    device_id_string = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    location_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scanners", x => x.scanner_id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    rfid_tag_uid = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    lastname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    role_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    item_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    rfid_uid = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    item_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    current_holder_id = table.Column<int>(type: "int", nullable: true),
                    last_updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.item_id);
                    table.ForeignKey(
                        name: "FK_Items_Users_current_holder_id",
                        column: x => x.current_holder_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    transaction_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    item_id = table.Column<int>(type: "int", nullable: false),
                    device_id = table.Column<int>(type: "int", nullable: false),
                    action = table.Column<int>(type: "int", nullable: false),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.transaction_id);
                    table.ForeignKey(
                        name: "FK_Transactions_Items_item_id",
                        column: x => x.item_id,
                        principalTable: "Items",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transactions_Scanners_device_id",
                        column: x => x.device_id,
                        principalTable: "Scanners",
                        principalColumn: "scanner_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transactions_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "item_id", "current_holder_id", "item_name", "last_updated", "rfid_uid", "status" },
                values: new object[,]
                {
                    { 1, null, "Power Drill", new DateTime(2025, 12, 9, 23, 7, 43, 452, DateTimeKind.Utc).AddTicks(2577), "RFID001234567890", 0 },
                    { 2, null, "Hammer", new DateTime(2025, 12, 9, 23, 7, 43, 452, DateTimeKind.Utc).AddTicks(2579), "RFID001234567891", 0 },
                    { 3, null, "Screwdriver Set", new DateTime(2025, 12, 9, 23, 7, 43, 452, DateTimeKind.Utc).AddTicks(2581), "RFID001234567892", 0 },
                    { 4, null, "Measuring Tape", new DateTime(2025, 12, 9, 23, 7, 43, 452, DateTimeKind.Utc).AddTicks(2582), "RFID001234567893", 0 },
                    { 5, null, "Wrench Set", new DateTime(2025, 12, 9, 23, 7, 43, 452, DateTimeKind.Utc).AddTicks(2583), "RFID001234567894", 0 }
                });

            migrationBuilder.InsertData(
                table: "Scanners",
                columns: new[] { "scanner_id", "device_id_string", "location_name" },
                values: new object[] { 1, "rpi-scanner-01", "Main Warehouse Entrance" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "user_id", "email", "lastname", "name", "password_hash", "rfid_tag_uid", "role_id" },
                values: new object[,]
                {
                    { 1, "admin@warehouse.com", "User", "Admin", "$2a$11$zSORvuaSdTGCnZrKnqMIqevxhWgClBa/1cBWEUAYYssz3wZmME8Ai", null, 1 },
                    { 2, "john.doe@warehouse.com", "Doe", "John", "$2a$11$x5y0/osEkY8EWBICD455nuB1lK/41fin0Yet1sPNk1eAaiJdEvpLK", null, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Items_current_holder_id",
                table: "Items",
                column: "current_holder_id");

            migrationBuilder.CreateIndex(
                name: "IX_Items_rfid_uid",
                table: "Items",
                column: "rfid_uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Scanners_device_id_string",
                table: "Scanners",
                column: "device_id_string",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_device_id",
                table: "Transactions",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_item_id",
                table: "Transactions",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_timestamp",
                table: "Transactions",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_user_id",
                table: "Transactions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_email",
                table: "Users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_rfid_tag_uid",
                table: "Users",
                column: "rfid_tag_uid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Scanners");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
