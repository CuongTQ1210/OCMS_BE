using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class adjustassignTablev3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignByUserId",
                table: "TraineeAssignments",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 13, 37, 24, 123, DateTimeKind.Utc).AddTicks(9253), new DateTime(2025, 3, 29, 20, 37, 24, 123, DateTimeKind.Local).AddTicks(9251) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 13, 37, 24, 346, DateTimeKind.Utc).AddTicks(3172), "$2a$11$Xed.eLyPdr/Sz8oYP52mSuKs53t06KpamUpyfzV15YAq48JPTKI.G", new DateTime(2025, 3, 29, 13, 37, 24, 346, DateTimeKind.Utc).AddTicks(3173) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 13, 37, 24, 346, DateTimeKind.Utc).AddTicks(3196), "$2a$11$hPz30qmmR26uRg874.W.0.Oix3wNRsFOI/lC/vXY7Yp3ELOj4qt9K", new DateTime(2025, 3, 29, 13, 37, 24, 346, DateTimeKind.Utc).AddTicks(3196) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 13, 37, 24, 346, DateTimeKind.Utc).AddTicks(3179), "$2a$11$hPz30qmmR26uRg874.W.0.Oix3wNRsFOI/lC/vXY7Yp3ELOj4qt9K", new DateTime(2025, 3, 29, 13, 37, 24, 346, DateTimeKind.Utc).AddTicks(3179) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 13, 37, 24, 346, DateTimeKind.Utc).AddTicks(3185), "$2a$11$hPz30qmmR26uRg874.W.0.Oix3wNRsFOI/lC/vXY7Yp3ELOj4qt9K", new DateTime(2025, 3, 29, 13, 37, 24, 346, DateTimeKind.Utc).AddTicks(3185) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 13, 37, 24, 346, DateTimeKind.Utc).AddTicks(3188), "$2a$11$hPz30qmmR26uRg874.W.0.Oix3wNRsFOI/lC/vXY7Yp3ELOj4qt9K", new DateTime(2025, 3, 29, 13, 37, 24, 346, DateTimeKind.Utc).AddTicks(3188) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 13, 37, 24, 346, DateTimeKind.Utc).AddTicks(3190), "$2a$11$hPz30qmmR26uRg874.W.0.Oix3wNRsFOI/lC/vXY7Yp3ELOj4qt9K", new DateTime(2025, 3, 29, 13, 37, 24, 346, DateTimeKind.Utc).AddTicks(3191) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 13, 37, 24, 346, DateTimeKind.Utc).AddTicks(3193), "$2a$11$hPz30qmmR26uRg874.W.0.Oix3wNRsFOI/lC/vXY7Yp3ELOj4qt9K", new DateTime(2025, 3, 29, 13, 37, 24, 346, DateTimeKind.Utc).AddTicks(3193) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 13, 37, 24, 346, DateTimeKind.Utc).AddTicks(3182), "$2a$11$hPz30qmmR26uRg874.W.0.Oix3wNRsFOI/lC/vXY7Yp3ELOj4qt9K", new DateTime(2025, 3, 29, 13, 37, 24, 346, DateTimeKind.Utc).AddTicks(3182) });

            migrationBuilder.CreateIndex(
                name: "IX_TraineeAssignments_AssignByUserId",
                table: "TraineeAssignments",
                column: "AssignByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TraineeAssignments_Users_AssignByUserId",
                table: "TraineeAssignments",
                column: "AssignByUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TraineeAssignments_Users_AssignByUserId",
                table: "TraineeAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TraineeAssignments_AssignByUserId",
                table: "TraineeAssignments");

            migrationBuilder.DropColumn(
                name: "AssignByUserId",
                table: "TraineeAssignments");

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
        }
    }
}
