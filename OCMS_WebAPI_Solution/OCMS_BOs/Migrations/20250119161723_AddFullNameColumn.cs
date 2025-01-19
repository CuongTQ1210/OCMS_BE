using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class AddFullNameColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedDate", "PasswordHash" },
                values: new object[] { new DateTime(2025, 1, 19, 16, 17, 23, 320, DateTimeKind.Utc).AddTicks(7513), "$2a$11$H7Kg9Gl7E36wELPq.tJbj.3HpSOGAcY7DKZ.93GT9KfKTCTYJ5Pe2" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedDate", "PasswordHash" },
                values: new object[] { new DateTime(2025, 1, 19, 12, 33, 58, 451, DateTimeKind.Utc).AddTicks(2198), "$2a$11$kKZ.ThFls9LjMf7OCUXvsOOe3s4uk.GOOnxUjZZ0esYkGHYOLz8Oq" });
        }
    }
}
