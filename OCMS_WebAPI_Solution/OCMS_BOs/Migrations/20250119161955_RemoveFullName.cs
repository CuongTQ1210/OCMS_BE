using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFullName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedDate", "PasswordHash" },
                values: new object[] { new DateTime(2025, 1, 19, 16, 19, 55, 573, DateTimeKind.Utc).AddTicks(9685), "$2a$11$MZ.o0cCvNMTE4bkY8qJdzOmsHXaOLlniyVdXS7mP//ft12SRn880y" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                values: new object[] { new DateTime(2025, 1, 19, 16, 17, 23, 320, DateTimeKind.Utc).AddTicks(7513), "Admin", "$2a$11$H7Kg9Gl7E36wELPq.tJbj.3HpSOGAcY7DKZ.93GT9KfKTCTYJ5Pe2" });
        }
    }
}
