using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class updatesubjectdatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Schedule",
                table: "Subjects");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Schedule",
                table: "Subjects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 21, 9, 14, 28, 663, DateTimeKind.Utc).AddTicks(3556), new DateTime(2025, 3, 21, 16, 14, 28, 663, DateTimeKind.Local).AddTicks(3554) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 21, 9, 14, 28, 916, DateTimeKind.Utc).AddTicks(3864), "$2a$11$y0f9j7uNQgVs7ChozdgcyuM5QxVcsdByuBnNR3HvNpv0KC.qE/9Bm", new DateTime(2025, 3, 21, 9, 14, 28, 916, DateTimeKind.Utc).AddTicks(3865) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 21, 9, 14, 28, 916, DateTimeKind.Utc).AddTicks(3898), "$2a$11$8tGixZX4TJEpJd9oKIpBO..AHSukZmY.5IFxVtVK4M3MEo7552zf.", new DateTime(2025, 3, 21, 9, 14, 28, 916, DateTimeKind.Utc).AddTicks(3898) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 21, 9, 14, 28, 916, DateTimeKind.Utc).AddTicks(3871), "$2a$11$8tGixZX4TJEpJd9oKIpBO..AHSukZmY.5IFxVtVK4M3MEo7552zf.", new DateTime(2025, 3, 21, 9, 14, 28, 916, DateTimeKind.Utc).AddTicks(3872) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 21, 9, 14, 28, 916, DateTimeKind.Utc).AddTicks(3879), "$2a$11$8tGixZX4TJEpJd9oKIpBO..AHSukZmY.5IFxVtVK4M3MEo7552zf.", new DateTime(2025, 3, 21, 9, 14, 28, 916, DateTimeKind.Utc).AddTicks(3879) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 21, 9, 14, 28, 916, DateTimeKind.Utc).AddTicks(3882), "$2a$11$8tGixZX4TJEpJd9oKIpBO..AHSukZmY.5IFxVtVK4M3MEo7552zf.", new DateTime(2025, 3, 21, 9, 14, 28, 916, DateTimeKind.Utc).AddTicks(3882) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 21, 9, 14, 28, 916, DateTimeKind.Utc).AddTicks(3884), "$2a$11$8tGixZX4TJEpJd9oKIpBO..AHSukZmY.5IFxVtVK4M3MEo7552zf.", new DateTime(2025, 3, 21, 9, 14, 28, 916, DateTimeKind.Utc).AddTicks(3885) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 21, 9, 14, 28, 916, DateTimeKind.Utc).AddTicks(3887), "$2a$11$8tGixZX4TJEpJd9oKIpBO..AHSukZmY.5IFxVtVK4M3MEo7552zf.", new DateTime(2025, 3, 21, 9, 14, 28, 916, DateTimeKind.Utc).AddTicks(3887) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 21, 9, 14, 28, 916, DateTimeKind.Utc).AddTicks(3875), "$2a$11$8tGixZX4TJEpJd9oKIpBO..AHSukZmY.5IFxVtVK4M3MEo7552zf.", new DateTime(2025, 3, 21, 9, 14, 28, 916, DateTimeKind.Utc).AddTicks(3875) });
        }
    }
}
