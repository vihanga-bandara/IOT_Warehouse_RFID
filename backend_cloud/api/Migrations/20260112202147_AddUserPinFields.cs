using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RfidWarehouseApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPinFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "pin_failed_attempts",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "pin_hash",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "pin_lockout_until",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "user_id",
                keyValue: 1,
                columns: new[] { "pin_failed_attempts", "pin_hash", "pin_lockout_until" },
                values: new object[] { 0, null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "user_id",
                keyValue: 2,
                columns: new[] { "pin_failed_attempts", "pin_hash", "pin_lockout_until" },
                values: new object[] { 0, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "pin_failed_attempts",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "pin_hash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "pin_lockout_until",
                table: "Users");
        }
    }
}
