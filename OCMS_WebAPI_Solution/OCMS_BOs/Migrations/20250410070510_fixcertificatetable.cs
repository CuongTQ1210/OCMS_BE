using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class fixcertificatetable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RevocationReason",
                table: "Certificates",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 10, 7, 5, 8, 569, DateTimeKind.Utc).AddTicks(2364), new DateTime(2025, 4, 10, 14, 5, 8, 569, DateTimeKind.Local).AddTicks(2363) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 10, 7, 5, 8, 810, DateTimeKind.Utc).AddTicks(4683), "$2a$11$JJgO2v5H1mY2jShzWzEememabbbETSqOz0Ucvp/7lWoYhzBenOxqe", new DateTime(2025, 4, 10, 7, 5, 8, 810, DateTimeKind.Utc).AddTicks(4684) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 10, 7, 5, 8, 810, DateTimeKind.Utc).AddTicks(4741), "$2a$11$frVBPjwYFvrgvv78Tc144esO8PIjp/jHoZheGYmIFxmk64b.Z5n4y", new DateTime(2025, 4, 10, 7, 5, 8, 810, DateTimeKind.Utc).AddTicks(4742) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 10, 7, 5, 8, 810, DateTimeKind.Utc).AddTicks(4689), "$2a$11$frVBPjwYFvrgvv78Tc144esO8PIjp/jHoZheGYmIFxmk64b.Z5n4y", new DateTime(2025, 4, 10, 7, 5, 8, 810, DateTimeKind.Utc).AddTicks(4689) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 10, 7, 5, 8, 810, DateTimeKind.Utc).AddTicks(4696), "$2a$11$frVBPjwYFvrgvv78Tc144esO8PIjp/jHoZheGYmIFxmk64b.Z5n4y", new DateTime(2025, 4, 10, 7, 5, 8, 810, DateTimeKind.Utc).AddTicks(4697) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 10, 7, 5, 8, 810, DateTimeKind.Utc).AddTicks(4731), "$2a$11$frVBPjwYFvrgvv78Tc144esO8PIjp/jHoZheGYmIFxmk64b.Z5n4y", new DateTime(2025, 4, 10, 7, 5, 8, 810, DateTimeKind.Utc).AddTicks(4731) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 10, 7, 5, 8, 810, DateTimeKind.Utc).AddTicks(4734), "$2a$11$frVBPjwYFvrgvv78Tc144esO8PIjp/jHoZheGYmIFxmk64b.Z5n4y", new DateTime(2025, 4, 10, 7, 5, 8, 810, DateTimeKind.Utc).AddTicks(4735) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 10, 7, 5, 8, 810, DateTimeKind.Utc).AddTicks(4738), "$2a$11$frVBPjwYFvrgvv78Tc144esO8PIjp/jHoZheGYmIFxmk64b.Z5n4y", new DateTime(2025, 4, 10, 7, 5, 8, 810, DateTimeKind.Utc).AddTicks(4738) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 10, 7, 5, 8, 810, DateTimeKind.Utc).AddTicks(4692), "$2a$11$frVBPjwYFvrgvv78Tc144esO8PIjp/jHoZheGYmIFxmk64b.Z5n4y", new DateTime(2025, 4, 10, 7, 5, 8, 810, DateTimeKind.Utc).AddTicks(4692) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RevocationReason",
                table: "Certificates",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

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
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3542), "$2a$11$DPFZOOo4X.Jbzk5AbfxZQez8zdOEiNQts/wSDivvn5NIWEXuw/P7i", new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3543) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3568), "$2a$11$0eJPfX/XEvgtoHVhmYgcLedW7TF2GUdz879H2uVcRb56XtrcDMqYu", new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3568) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3547), "$2a$11$0eJPfX/XEvgtoHVhmYgcLedW7TF2GUdz879H2uVcRb56XtrcDMqYu", new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3547) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3553), "$2a$11$0eJPfX/XEvgtoHVhmYgcLedW7TF2GUdz879H2uVcRb56XtrcDMqYu", new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3554) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3560), "$2a$11$0eJPfX/XEvgtoHVhmYgcLedW7TF2GUdz879H2uVcRb56XtrcDMqYu", new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3560) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3562), "$2a$11$0eJPfX/XEvgtoHVhmYgcLedW7TF2GUdz879H2uVcRb56XtrcDMqYu", new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3563) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3565), "$2a$11$0eJPfX/XEvgtoHVhmYgcLedW7TF2GUdz879H2uVcRb56XtrcDMqYu", new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3565) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3550), "$2a$11$0eJPfX/XEvgtoHVhmYgcLedW7TF2GUdz879H2uVcRb56XtrcDMqYu", new DateTime(2025, 4, 10, 6, 17, 12, 164, DateTimeKind.Utc).AddTicks(3550) });
        }
    }
}
