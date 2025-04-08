using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class fixtraineeassigntable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "TraineeAssignments",
                type: "text",
                nullable: false,
                defaultValue: "");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "TraineeAssignments");

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
    }
}
