using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class addAvatar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 10, 6, 17, 11, 922, DateTimeKind.Utc).AddTicks(8902), new DateTime(2025, 4, 10, 13, 17, 11, 922, DateTimeKind.Local).AddTicks(8901) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "AvatarUrl", "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { "", new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3542), "$2a$11$DPFZOOo4X.Jbzk5AbfxZQez8zdOEiNQts/wSDivvn5NIWEXuw/P7i", new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3543) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "AvatarUrl", "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { "", new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3568), "$2a$11$0eJPfX/XEvgtoHVhmYgcLedW7TF2GUdz879H2uVcRb56XtrcDMqYu", new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3568) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "AvatarUrl", "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { "", new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3547), "$2a$11$0eJPfX/XEvgtoHVhmYgcLedW7TF2GUdz879H2uVcRb56XtrcDMqYu", new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3547) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "AvatarUrl", "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { "", new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3553), "$2a$11$0eJPfX/XEvgtoHVhmYgcLedW7TF2GUdz879H2uVcRb56XtrcDMqYu", new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3554) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "AvatarUrl", "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { "", new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3560), "$2a$11$0eJPfX/XEvgtoHVhmYgcLedW7TF2GUdz879H2uVcRb56XtrcDMqYu", new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3560) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "AvatarUrl", "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { "", new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3562), "$2a$11$0eJPfX/XEvgtoHVhmYgcLedW7TF2GUdz879H2uVcRb56XtrcDMqYu", new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3563) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "AvatarUrl", "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { "", new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3565), "$2a$11$0eJPfX/XEvgtoHVhmYgcLedW7TF2GUdz879H2uVcRb56XtrcDMqYu", new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3565) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "AvatarUrl", "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { "", new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3550), "$2a$11$0eJPfX/XEvgtoHVhmYgcLedW7TF2GUdz879H2uVcRb56XtrcDMqYu", new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3550) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 17, 31, 1, 53, DateTimeKind.Utc).AddTicks(520), new DateTime(2025, 4, 10, 0, 31, 1, 53, DateTimeKind.Local).AddTicks(517) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1907), "$2a$11$2UzEqQy98E..PqCvIIf1QuImaGNLTsPPFxLNkBQZp2U9sRax8UkC.", new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1909) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1929), "$2a$11$Evyl/DUU6qjnDmbglyLIkuNzkqmfYcYTB2zAvGu4/WzeyRaESe782", new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1929) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1913), "$2a$11$Evyl/DUU6qjnDmbglyLIkuNzkqmfYcYTB2zAvGu4/WzeyRaESe782", new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1914) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1919), "$2a$11$Evyl/DUU6qjnDmbglyLIkuNzkqmfYcYTB2zAvGu4/WzeyRaESe782", new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1920) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1922), "$2a$11$Evyl/DUU6qjnDmbglyLIkuNzkqmfYcYTB2zAvGu4/WzeyRaESe782", new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1922) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1924), "$2a$11$Evyl/DUU6qjnDmbglyLIkuNzkqmfYcYTB2zAvGu4/WzeyRaESe782", new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1924) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1927), "$2a$11$Evyl/DUU6qjnDmbglyLIkuNzkqmfYcYTB2zAvGu4/WzeyRaESe782", new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1927) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1917), "$2a$11$Evyl/DUU6qjnDmbglyLIkuNzkqmfYcYTB2zAvGu4/WzeyRaESe782", new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1917) });
        }
    }
}
