using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class newdbb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedDate", "FullName", "PasswordHash" },
                values: new object[] { new DateTime(2025, 1, 19, 12, 33, 58, 451, DateTimeKind.Utc).AddTicks(2198), "Admin", "$2a$11$kKZ.ThFls9LjMf7OCUXvsOOe3s4uk.GOOnxUjZZ0esYkGHYOLz8Oq" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedDate", "PasswordHash" },
                values: new object[] { new DateTime(2025, 1, 19, 10, 35, 51, 669, DateTimeKind.Utc).AddTicks(2130), "$2a$11$HpKC6kVjQhZFFUr3KP5rLOajZ9PtguWRCshlA6BSPuLRU1W/3lEnC" });
        }
    }
}
