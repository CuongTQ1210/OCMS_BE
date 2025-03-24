using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class adjustdatabasefortraining : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "TrainingSchedules",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "TrainingSchedules",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 3, 52, 16, 645, DateTimeKind.Utc).AddTicks(8267), new DateTime(2025, 3, 17, 10, 52, 16, 645, DateTimeKind.Local).AddTicks(8263) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6885), "$2a$11$.7A86hY53MBXpO5xtfRiFekdRIVVH81DfcP3p.TcgDBQSh9vDQXRK", new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6885) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6925), "$2a$11$PQS15gyKR12c49tEDtQM6e4pluu1PBXiGKHmGJ.wCXF8AXwbTil4O", new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6925) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6891), "$2a$11$PQS15gyKR12c49tEDtQM6e4pluu1PBXiGKHmGJ.wCXF8AXwbTil4O", new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6891) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6897), "$2a$11$PQS15gyKR12c49tEDtQM6e4pluu1PBXiGKHmGJ.wCXF8AXwbTil4O", new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6898) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6900), "$2a$11$PQS15gyKR12c49tEDtQM6e4pluu1PBXiGKHmGJ.wCXF8AXwbTil4O", new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6901) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6903), "$2a$11$PQS15gyKR12c49tEDtQM6e4pluu1PBXiGKHmGJ.wCXF8AXwbTil4O", new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6904) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6921), "$2a$11$PQS15gyKR12c49tEDtQM6e4pluu1PBXiGKHmGJ.wCXF8AXwbTil4O", new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6922) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6894), "$2a$11$PQS15gyKR12c49tEDtQM6e4pluu1PBXiGKHmGJ.wCXF8AXwbTil4O", new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6894) });
        }
    }
}
