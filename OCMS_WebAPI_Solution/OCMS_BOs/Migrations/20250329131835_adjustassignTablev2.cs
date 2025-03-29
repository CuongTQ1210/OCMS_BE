using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class adjustassignTablev2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestId",
                table: "TraineeAssignments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 13, 18, 34, 442, DateTimeKind.Utc).AddTicks(4331), new DateTime(2025, 3, 29, 20, 18, 34, 442, DateTimeKind.Local).AddTicks(4329) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 13, 18, 34, 777, DateTimeKind.Utc).AddTicks(1765), "$2a$11$oO8vO..ogH9lmX4Vz3RhB.05.SpitDIpfq0Aw6AzVumE/CW127Zri", new DateTime(2025, 3, 29, 13, 18, 34, 777, DateTimeKind.Utc).AddTicks(1767) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 13, 18, 34, 777, DateTimeKind.Utc).AddTicks(1788), "$2a$11$LJJe0qFBe80VySlgoIb2nePDPE7ushSi67vTG8hu3nkrF3.2LujKe", new DateTime(2025, 3, 29, 13, 18, 34, 777, DateTimeKind.Utc).AddTicks(1788) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 13, 18, 34, 777, DateTimeKind.Utc).AddTicks(1772), "$2a$11$LJJe0qFBe80VySlgoIb2nePDPE7ushSi67vTG8hu3nkrF3.2LujKe", new DateTime(2025, 3, 29, 13, 18, 34, 777, DateTimeKind.Utc).AddTicks(1772) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 13, 18, 34, 777, DateTimeKind.Utc).AddTicks(1778), "$2a$11$LJJe0qFBe80VySlgoIb2nePDPE7ushSi67vTG8hu3nkrF3.2LujKe", new DateTime(2025, 3, 29, 13, 18, 34, 777, DateTimeKind.Utc).AddTicks(1778) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 13, 18, 34, 777, DateTimeKind.Utc).AddTicks(1780), "$2a$11$LJJe0qFBe80VySlgoIb2nePDPE7ushSi67vTG8hu3nkrF3.2LujKe", new DateTime(2025, 3, 29, 13, 18, 34, 777, DateTimeKind.Utc).AddTicks(1781) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 13, 18, 34, 777, DateTimeKind.Utc).AddTicks(1783), "$2a$11$LJJe0qFBe80VySlgoIb2nePDPE7ushSi67vTG8hu3nkrF3.2LujKe", new DateTime(2025, 3, 29, 13, 18, 34, 777, DateTimeKind.Utc).AddTicks(1783) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 13, 18, 34, 777, DateTimeKind.Utc).AddTicks(1785), "$2a$11$LJJe0qFBe80VySlgoIb2nePDPE7ushSi67vTG8hu3nkrF3.2LujKe", new DateTime(2025, 3, 29, 13, 18, 34, 777, DateTimeKind.Utc).AddTicks(1786) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 13, 18, 34, 777, DateTimeKind.Utc).AddTicks(1775), "$2a$11$LJJe0qFBe80VySlgoIb2nePDPE7ushSi67vTG8hu3nkrF3.2LujKe", new DateTime(2025, 3, 29, 13, 18, 34, 777, DateTimeKind.Utc).AddTicks(1775) });

            migrationBuilder.CreateIndex(
                name: "IX_TraineeAssignments_RequestId",
                table: "TraineeAssignments",
                column: "RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_TraineeAssignments_Requests_RequestId",
                table: "TraineeAssignments",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "RequestId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TraineeAssignments_Requests_RequestId",
                table: "TraineeAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TraineeAssignments_RequestId",
                table: "TraineeAssignments");

            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "TraineeAssignments");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 12, 45, 44, 818, DateTimeKind.Utc).AddTicks(380), new DateTime(2025, 3, 29, 19, 45, 44, 818, DateTimeKind.Local).AddTicks(378) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3129), "$2a$11$NWKrYk5kJQdL8Fm6EcIY8.bxHjHXvw1nPQ6nGbK/z2LZKVQdQ8Fhu", new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3131) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3155), "$2a$11$VtOmofIfis/bq/GdqLq0J.43q0UlAtskF4CMuEf9d1w.FZFm0pzWe", new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3156) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3136), "$2a$11$VtOmofIfis/bq/GdqLq0J.43q0UlAtskF4CMuEf9d1w.FZFm0pzWe", new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3137) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3144), "$2a$11$VtOmofIfis/bq/GdqLq0J.43q0UlAtskF4CMuEf9d1w.FZFm0pzWe", new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3144) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3147), "$2a$11$VtOmofIfis/bq/GdqLq0J.43q0UlAtskF4CMuEf9d1w.FZFm0pzWe", new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3147) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3150), "$2a$11$VtOmofIfis/bq/GdqLq0J.43q0UlAtskF4CMuEf9d1w.FZFm0pzWe", new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3150) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3152), "$2a$11$VtOmofIfis/bq/GdqLq0J.43q0UlAtskF4CMuEf9d1w.FZFm0pzWe", new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3153) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3140), "$2a$11$VtOmofIfis/bq/GdqLq0J.43q0UlAtskF4CMuEf9d1w.FZFm0pzWe", new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3140) });
        }
    }
}
