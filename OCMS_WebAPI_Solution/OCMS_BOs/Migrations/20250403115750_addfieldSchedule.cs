using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class addfieldSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "ClassTime",
                table: "TrainingSchedules",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<int[]>(
                name: "DaysOfWeek",
                table: "TrainingSchedules",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 3, 11, 57, 49, 530, DateTimeKind.Utc).AddTicks(3233), new DateTime(2025, 4, 3, 18, 57, 49, 530, DateTimeKind.Local).AddTicks(3231) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 3, 11, 57, 49, 881, DateTimeKind.Utc).AddTicks(1744), "$2a$11$XqyQrcqavlD5j17lb7o3b.eI5G2M4npjBtVdEd6ONKe.ka7rYBqfG", new DateTime(2025, 4, 3, 11, 57, 49, 881, DateTimeKind.Utc).AddTicks(1744) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 3, 11, 57, 49, 881, DateTimeKind.Utc).AddTicks(1765), "$2a$11$5FuEdSbyGimeLy94bvNIq.Ed2QnPTYDuoCbgZITTmCXkaMZQy81oS", new DateTime(2025, 4, 3, 11, 57, 49, 881, DateTimeKind.Utc).AddTicks(1766) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 3, 11, 57, 49, 881, DateTimeKind.Utc).AddTicks(1750), "$2a$11$5FuEdSbyGimeLy94bvNIq.Ed2QnPTYDuoCbgZITTmCXkaMZQy81oS", new DateTime(2025, 4, 3, 11, 57, 49, 881, DateTimeKind.Utc).AddTicks(1751) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 3, 11, 57, 49, 881, DateTimeKind.Utc).AddTicks(1756), "$2a$11$5FuEdSbyGimeLy94bvNIq.Ed2QnPTYDuoCbgZITTmCXkaMZQy81oS", new DateTime(2025, 4, 3, 11, 57, 49, 881, DateTimeKind.Utc).AddTicks(1756) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 3, 11, 57, 49, 881, DateTimeKind.Utc).AddTicks(1758), "$2a$11$5FuEdSbyGimeLy94bvNIq.Ed2QnPTYDuoCbgZITTmCXkaMZQy81oS", new DateTime(2025, 4, 3, 11, 57, 49, 881, DateTimeKind.Utc).AddTicks(1758) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 3, 11, 57, 49, 881, DateTimeKind.Utc).AddTicks(1761), "$2a$11$5FuEdSbyGimeLy94bvNIq.Ed2QnPTYDuoCbgZITTmCXkaMZQy81oS", new DateTime(2025, 4, 3, 11, 57, 49, 881, DateTimeKind.Utc).AddTicks(1761) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 3, 11, 57, 49, 881, DateTimeKind.Utc).AddTicks(1763), "$2a$11$5FuEdSbyGimeLy94bvNIq.Ed2QnPTYDuoCbgZITTmCXkaMZQy81oS", new DateTime(2025, 4, 3, 11, 57, 49, 881, DateTimeKind.Utc).AddTicks(1763) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 3, 11, 57, 49, 881, DateTimeKind.Utc).AddTicks(1753), "$2a$11$5FuEdSbyGimeLy94bvNIq.Ed2QnPTYDuoCbgZITTmCXkaMZQy81oS", new DateTime(2025, 4, 3, 11, 57, 49, 881, DateTimeKind.Utc).AddTicks(1753) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClassTime",
                table: "TrainingSchedules");

            migrationBuilder.DropColumn(
                name: "DaysOfWeek",
                table: "TrainingSchedules");

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
        }
    }
}
