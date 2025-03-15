using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class adjustrequesttable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Requests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 15, 10, 24, 32, 831, DateTimeKind.Utc).AddTicks(674), new DateTime(2025, 3, 15, 17, 24, 32, 831, DateTimeKind.Local).AddTicks(671) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 15, 10, 24, 33, 28, DateTimeKind.Utc).AddTicks(9476), "$2a$11$xVxE2k8CxoOCp1L7yWS7IeeoF/exGGFhuknwpXnwZGL6BXiE4CSH.", new DateTime(2025, 3, 15, 10, 24, 33, 28, DateTimeKind.Utc).AddTicks(9476) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Requests");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 14, 18, 43, 46, 246, DateTimeKind.Utc).AddTicks(9115), new DateTime(2025, 3, 15, 1, 43, 46, 246, DateTimeKind.Local).AddTicks(9115) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 14, 18, 43, 46, 371, DateTimeKind.Utc).AddTicks(4468), "$2a$11$0LHlJnSpv3ZqhXKDwgv1peGJnnmFEK686KbT2KQEDsb1xf6zFvOaS", new DateTime(2025, 3, 14, 18, 43, 46, 371, DateTimeKind.Utc).AddTicks(4469) });
        }
    }
}
