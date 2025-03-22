using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class updateSubjectDBv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 15, 5, 57, 260, DateTimeKind.Utc).AddTicks(6131), new DateTime(2025, 3, 22, 22, 5, 57, 260, DateTimeKind.Local).AddTicks(6129) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3022), "$2a$11$ezUZulbPMeaHZWV3/I2v4OaG7KI7Be27gVY4Zrb8EpcwTHMXJ4Rxm", new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3023) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3169), "$2a$11$8KCXkxpg4d58M4FX7hVkVONUpSmq1hmT999j02HVE4TE/7Qapjq5i", new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3169) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3030), "$2a$11$8KCXkxpg4d58M4FX7hVkVONUpSmq1hmT999j02HVE4TE/7Qapjq5i", new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3031) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3039), "$2a$11$8KCXkxpg4d58M4FX7hVkVONUpSmq1hmT999j02HVE4TE/7Qapjq5i", new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3039) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3144), "$2a$11$8KCXkxpg4d58M4FX7hVkVONUpSmq1hmT999j02HVE4TE/7Qapjq5i", new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3145) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3148), "$2a$11$8KCXkxpg4d58M4FX7hVkVONUpSmq1hmT999j02HVE4TE/7Qapjq5i", new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3148) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3165), "$2a$11$8KCXkxpg4d58M4FX7hVkVONUpSmq1hmT999j02HVE4TE/7Qapjq5i", new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3165) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3035), "$2a$11$8KCXkxpg4d58M4FX7hVkVONUpSmq1hmT999j02HVE4TE/7Qapjq5i", new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3035) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 14, 57, 50, 882, DateTimeKind.Utc).AddTicks(9655), new DateTime(2025, 3, 22, 21, 57, 50, 882, DateTimeKind.Local).AddTicks(9653) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 14, 57, 51, 147, DateTimeKind.Utc).AddTicks(4889), "$2a$11$90.HKmzhbAiI/J5arjh3Z.TVwiIABUS/Q0ED.68oOY0gtqTVtfoQK", new DateTime(2025, 3, 22, 14, 57, 51, 147, DateTimeKind.Utc).AddTicks(4892) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 14, 57, 51, 147, DateTimeKind.Utc).AddTicks(5142), "$2a$11$9COcNsG3dlAB/Dq2ZdeFeOL4RL9mQZ511FQfY5vVoCEoZtIiD9mgK", new DateTime(2025, 3, 22, 14, 57, 51, 147, DateTimeKind.Utc).AddTicks(5143) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 14, 57, 51, 147, DateTimeKind.Utc).AddTicks(4899), "$2a$11$9COcNsG3dlAB/Dq2ZdeFeOL4RL9mQZ511FQfY5vVoCEoZtIiD9mgK", new DateTime(2025, 3, 22, 14, 57, 51, 147, DateTimeKind.Utc).AddTicks(4899) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 14, 57, 51, 147, DateTimeKind.Utc).AddTicks(4907), "$2a$11$9COcNsG3dlAB/Dq2ZdeFeOL4RL9mQZ511FQfY5vVoCEoZtIiD9mgK", new DateTime(2025, 3, 22, 14, 57, 51, 147, DateTimeKind.Utc).AddTicks(4908) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 14, 57, 51, 147, DateTimeKind.Utc).AddTicks(4911), "$2a$11$9COcNsG3dlAB/Dq2ZdeFeOL4RL9mQZ511FQfY5vVoCEoZtIiD9mgK", new DateTime(2025, 3, 22, 14, 57, 51, 147, DateTimeKind.Utc).AddTicks(4911) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 14, 57, 51, 147, DateTimeKind.Utc).AddTicks(5113), "$2a$11$9COcNsG3dlAB/Dq2ZdeFeOL4RL9mQZ511FQfY5vVoCEoZtIiD9mgK", new DateTime(2025, 3, 22, 14, 57, 51, 147, DateTimeKind.Utc).AddTicks(5114) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 14, 57, 51, 147, DateTimeKind.Utc).AddTicks(5138), "$2a$11$9COcNsG3dlAB/Dq2ZdeFeOL4RL9mQZ511FQfY5vVoCEoZtIiD9mgK", new DateTime(2025, 3, 22, 14, 57, 51, 147, DateTimeKind.Utc).AddTicks(5139) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 14, 57, 51, 147, DateTimeKind.Utc).AddTicks(4903), "$2a$11$9COcNsG3dlAB/Dq2ZdeFeOL4RL9mQZ511FQfY5vVoCEoZtIiD9mgK", new DateTime(2025, 3, 22, 14, 57, 51, 147, DateTimeKind.Utc).AddTicks(4903) });
        }
    }
}
