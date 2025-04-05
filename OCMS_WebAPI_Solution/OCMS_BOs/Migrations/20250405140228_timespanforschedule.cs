using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class timespanforschedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "SubjectPeriod",
                table: "TrainingSchedules",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 5, 14, 2, 27, 111, DateTimeKind.Utc).AddTicks(6882), new DateTime(2025, 4, 5, 21, 2, 27, 111, DateTimeKind.Local).AddTicks(6880) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 5, 14, 2, 27, 520, DateTimeKind.Utc).AddTicks(3247), "$2a$11$.MBJxThgggeSweEACqhDJ.BGnJ5p.cc3G5UOfTFGTdM.0hVeCe2ri", new DateTime(2025, 4, 5, 14, 2, 27, 520, DateTimeKind.Utc).AddTicks(3249) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 5, 14, 2, 27, 520, DateTimeKind.Utc).AddTicks(3273), "$2a$11$1fTq5r5/nhXMYtjn212l6OOfuhCLMltBBzFQPbSlkvh1O9mVrBfDW", new DateTime(2025, 4, 5, 14, 2, 27, 520, DateTimeKind.Utc).AddTicks(3274) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 5, 14, 2, 27, 520, DateTimeKind.Utc).AddTicks(3255), "$2a$11$1fTq5r5/nhXMYtjn212l6OOfuhCLMltBBzFQPbSlkvh1O9mVrBfDW", new DateTime(2025, 4, 5, 14, 2, 27, 520, DateTimeKind.Utc).AddTicks(3255) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 5, 14, 2, 27, 520, DateTimeKind.Utc).AddTicks(3262), "$2a$11$1fTq5r5/nhXMYtjn212l6OOfuhCLMltBBzFQPbSlkvh1O9mVrBfDW", new DateTime(2025, 4, 5, 14, 2, 27, 520, DateTimeKind.Utc).AddTicks(3262) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 5, 14, 2, 27, 520, DateTimeKind.Utc).AddTicks(3265), "$2a$11$1fTq5r5/nhXMYtjn212l6OOfuhCLMltBBzFQPbSlkvh1O9mVrBfDW", new DateTime(2025, 4, 5, 14, 2, 27, 520, DateTimeKind.Utc).AddTicks(3265) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 5, 14, 2, 27, 520, DateTimeKind.Utc).AddTicks(3268), "$2a$11$1fTq5r5/nhXMYtjn212l6OOfuhCLMltBBzFQPbSlkvh1O9mVrBfDW", new DateTime(2025, 4, 5, 14, 2, 27, 520, DateTimeKind.Utc).AddTicks(3268) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 5, 14, 2, 27, 520, DateTimeKind.Utc).AddTicks(3270), "$2a$11$1fTq5r5/nhXMYtjn212l6OOfuhCLMltBBzFQPbSlkvh1O9mVrBfDW", new DateTime(2025, 4, 5, 14, 2, 27, 520, DateTimeKind.Utc).AddTicks(3271) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 5, 14, 2, 27, 520, DateTimeKind.Utc).AddTicks(3258), "$2a$11$1fTq5r5/nhXMYtjn212l6OOfuhCLMltBBzFQPbSlkvh1O9mVrBfDW", new DateTime(2025, 4, 5, 14, 2, 27, 520, DateTimeKind.Utc).AddTicks(3259) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubjectPeriod",
                table: "TrainingSchedules");

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
    }
}
