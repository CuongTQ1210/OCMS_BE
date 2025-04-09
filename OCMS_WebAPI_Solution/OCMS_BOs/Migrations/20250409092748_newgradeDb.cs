using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class newgradeDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Score",
                table: "Grades",
                newName: "TotalScore");

            migrationBuilder.AddColumn<double>(
                name: "AssignmentScore",
                table: "Grades",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FinalExamScore",
                table: "Grades",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FinalResitScore",
                table: "Grades",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ParticipantScore",
                table: "Grades",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 9, 27, 47, 902, DateTimeKind.Utc).AddTicks(6440), new DateTime(2025, 4, 9, 16, 27, 47, 902, DateTimeKind.Local).AddTicks(6438) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1664), "$2a$11$DgmbZsslMnVS8e8KbFX5A.7EkCFiWDKQpP6xgBQopV5QwPY7tfP2e", new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1665) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1687), "$2a$11$kQhGvVjiYNdLYClynw/ac.dW4d1LJyyQucwXTyMAl6HpgLZu0OZKm", new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1688) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1670), "$2a$11$kQhGvVjiYNdLYClynw/ac.dW4d1LJyyQucwXTyMAl6HpgLZu0OZKm", new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1671) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1676), "$2a$11$kQhGvVjiYNdLYClynw/ac.dW4d1LJyyQucwXTyMAl6HpgLZu0OZKm", new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1677) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1679), "$2a$11$kQhGvVjiYNdLYClynw/ac.dW4d1LJyyQucwXTyMAl6HpgLZu0OZKm", new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1680) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1682), "$2a$11$kQhGvVjiYNdLYClynw/ac.dW4d1LJyyQucwXTyMAl6HpgLZu0OZKm", new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1682) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1685), "$2a$11$kQhGvVjiYNdLYClynw/ac.dW4d1LJyyQucwXTyMAl6HpgLZu0OZKm", new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1685) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1674), "$2a$11$kQhGvVjiYNdLYClynw/ac.dW4d1LJyyQucwXTyMAl6HpgLZu0OZKm", new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1674) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignmentScore",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "FinalExamScore",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "FinalResitScore",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "ParticipantScore",
                table: "Grades");

            migrationBuilder.RenameColumn(
                name: "TotalScore",
                table: "Grades",
                newName: "Score");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 8, 6, 35, 7, 526, DateTimeKind.Utc).AddTicks(917), new DateTime(2025, 4, 8, 13, 35, 7, 526, DateTimeKind.Local).AddTicks(915) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 8, 6, 35, 7, 778, DateTimeKind.Utc).AddTicks(7710), "$2a$11$1U8FyZx3ibSXecKuRsvhxOrIRnTDbubv5UXACKWOItz7gMBLEEaSG", new DateTime(2025, 4, 8, 6, 35, 7, 778, DateTimeKind.Utc).AddTicks(7711) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 8, 6, 35, 7, 778, DateTimeKind.Utc).AddTicks(7738), "$2a$11$yebhjXOoqROVAPuYyYIFquHWCMqneWuP4q32v2l16z9s4tn2ERVmy", new DateTime(2025, 4, 8, 6, 35, 7, 778, DateTimeKind.Utc).AddTicks(7739) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 8, 6, 35, 7, 778, DateTimeKind.Utc).AddTicks(7718), "$2a$11$yebhjXOoqROVAPuYyYIFquHWCMqneWuP4q32v2l16z9s4tn2ERVmy", new DateTime(2025, 4, 8, 6, 35, 7, 778, DateTimeKind.Utc).AddTicks(7718) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 8, 6, 35, 7, 778, DateTimeKind.Utc).AddTicks(7725), "$2a$11$yebhjXOoqROVAPuYyYIFquHWCMqneWuP4q32v2l16z9s4tn2ERVmy", new DateTime(2025, 4, 8, 6, 35, 7, 778, DateTimeKind.Utc).AddTicks(7726) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 8, 6, 35, 7, 778, DateTimeKind.Utc).AddTicks(7729), "$2a$11$yebhjXOoqROVAPuYyYIFquHWCMqneWuP4q32v2l16z9s4tn2ERVmy", new DateTime(2025, 4, 8, 6, 35, 7, 778, DateTimeKind.Utc).AddTicks(7729) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 8, 6, 35, 7, 778, DateTimeKind.Utc).AddTicks(7732), "$2a$11$yebhjXOoqROVAPuYyYIFquHWCMqneWuP4q32v2l16z9s4tn2ERVmy", new DateTime(2025, 4, 8, 6, 35, 7, 778, DateTimeKind.Utc).AddTicks(7732) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 8, 6, 35, 7, 778, DateTimeKind.Utc).AddTicks(7735), "$2a$11$yebhjXOoqROVAPuYyYIFquHWCMqneWuP4q32v2l16z9s4tn2ERVmy", new DateTime(2025, 4, 8, 6, 35, 7, 778, DateTimeKind.Utc).AddTicks(7735) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 8, 6, 35, 7, 778, DateTimeKind.Utc).AddTicks(7722), "$2a$11$yebhjXOoqROVAPuYyYIFquHWCMqneWuP4q32v2l16z9s4tn2ERVmy", new DateTime(2025, 4, 8, 6, 35, 7, 778, DateTimeKind.Utc).AddTicks(7722) });
        }
    }
}
