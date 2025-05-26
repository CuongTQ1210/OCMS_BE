using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseidtoClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CourseId",
                table: "Class",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 26, 1, 18, 50, 421, DateTimeKind.Utc).AddTicks(1810), new DateTime(2025, 5, 26, 8, 18, 50, 421, DateTimeKind.Local).AddTicks(1808) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5549), "$2a$11$GFMBWumalt82dUJXDdlSculVFWHMoE1tGKHOSyDrH41wSOSNYjcDq", new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5550) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5587), "$2a$11$4hXwaBMOHNZvkoRe9RvU1e1JubiHBEEvkjLc2C6Qk74uTkMO0NoPO", new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5588) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5556), "$2a$11$4hXwaBMOHNZvkoRe9RvU1e1JubiHBEEvkjLc2C6Qk74uTkMO0NoPO", new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5557) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5562), "$2a$11$4hXwaBMOHNZvkoRe9RvU1e1JubiHBEEvkjLc2C6Qk74uTkMO0NoPO", new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5563) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5565), "$2a$11$4hXwaBMOHNZvkoRe9RvU1e1JubiHBEEvkjLc2C6Qk74uTkMO0NoPO", new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5566) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5568), "$2a$11$4hXwaBMOHNZvkoRe9RvU1e1JubiHBEEvkjLc2C6Qk74uTkMO0NoPO", new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5568) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5571), "$2a$11$4hXwaBMOHNZvkoRe9RvU1e1JubiHBEEvkjLc2C6Qk74uTkMO0NoPO", new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5571) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5559), "$2a$11$4hXwaBMOHNZvkoRe9RvU1e1JubiHBEEvkjLc2C6Qk74uTkMO0NoPO", new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5560) });

            migrationBuilder.CreateIndex(
                name: "IX_Class_CourseId",
                table: "Class",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Class_Courses_CourseId",
                table: "Class",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Class_Courses_CourseId",
                table: "Class");

            migrationBuilder.DropIndex(
                name: "IX_Class_CourseId",
                table: "Class");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Class");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 23, 10, 22, 51, 781, DateTimeKind.Utc).AddTicks(4575), new DateTime(2025, 5, 23, 17, 22, 51, 781, DateTimeKind.Local).AddTicks(4572) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2036), "$2a$11$tdmMZxn9DjG6L4rDksUz5eWY7TZDT8CNwS6uXK0fz9rQ7XkE9xrem", new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2037) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2117), "$2a$11$IHV5eWxC4xE2ESo3EAuS/exwN6.PHe8duc6N2KRCIIndh5y/A56we", new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2118) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2046), "$2a$11$IHV5eWxC4xE2ESo3EAuS/exwN6.PHe8duc6N2KRCIIndh5y/A56we", new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2047) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2060), "$2a$11$IHV5eWxC4xE2ESo3EAuS/exwN6.PHe8duc6N2KRCIIndh5y/A56we", new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2061) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2067), "$2a$11$IHV5eWxC4xE2ESo3EAuS/exwN6.PHe8duc6N2KRCIIndh5y/A56we", new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2068) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2075), "$2a$11$IHV5eWxC4xE2ESo3EAuS/exwN6.PHe8duc6N2KRCIIndh5y/A56we", new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2075) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2109), "$2a$11$IHV5eWxC4xE2ESo3EAuS/exwN6.PHe8duc6N2KRCIIndh5y/A56we", new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2111) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2053), "$2a$11$IHV5eWxC4xE2ESo3EAuS/exwN6.PHe8duc6N2KRCIIndh5y/A56we", new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2054) });
        }
    }
}
