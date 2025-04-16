using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class MakeApproveByUserIdNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DecisionTemplate_Users_ApprovedByUserId",
                table: "DecisionTemplate");

            migrationBuilder.AlterColumn<string>(
                name: "ApprovedByUserId",
                table: "DecisionTemplate",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 15, 20, 9, 29, 886, DateTimeKind.Utc).AddTicks(6291), new DateTime(2025, 4, 16, 3, 9, 29, 886, DateTimeKind.Local).AddTicks(6289) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 15, 20, 9, 30, 242, DateTimeKind.Utc).AddTicks(9812), "$2a$11$EiXCXpCi5xAdi7sLTO.Ta.pMwAQP/HBrCX4qiN5oZ3nFaJoIOLIP6", new DateTime(2025, 4, 15, 20, 9, 30, 242, DateTimeKind.Utc).AddTicks(9813) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 15, 20, 9, 30, 242, DateTimeKind.Utc).AddTicks(9910), "$2a$11$TxRzazTkfYE7iu6D8t7fbOmi9KUL2zedwgF95iLwI.zAFSApwj14i", new DateTime(2025, 4, 15, 20, 9, 30, 242, DateTimeKind.Utc).AddTicks(9910) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 15, 20, 9, 30, 242, DateTimeKind.Utc).AddTicks(9822), "$2a$11$TxRzazTkfYE7iu6D8t7fbOmi9KUL2zedwgF95iLwI.zAFSApwj14i", new DateTime(2025, 4, 15, 20, 9, 30, 242, DateTimeKind.Utc).AddTicks(9822) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 15, 20, 9, 30, 242, DateTimeKind.Utc).AddTicks(9837), "$2a$11$TxRzazTkfYE7iu6D8t7fbOmi9KUL2zedwgF95iLwI.zAFSApwj14i", new DateTime(2025, 4, 15, 20, 9, 30, 242, DateTimeKind.Utc).AddTicks(9838) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 15, 20, 9, 30, 242, DateTimeKind.Utc).AddTicks(9842), "$2a$11$TxRzazTkfYE7iu6D8t7fbOmi9KUL2zedwgF95iLwI.zAFSApwj14i", new DateTime(2025, 4, 15, 20, 9, 30, 242, DateTimeKind.Utc).AddTicks(9843) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 15, 20, 9, 30, 242, DateTimeKind.Utc).AddTicks(9847), "$2a$11$TxRzazTkfYE7iu6D8t7fbOmi9KUL2zedwgF95iLwI.zAFSApwj14i", new DateTime(2025, 4, 15, 20, 9, 30, 242, DateTimeKind.Utc).AddTicks(9847) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 15, 20, 9, 30, 242, DateTimeKind.Utc).AddTicks(9905), "$2a$11$TxRzazTkfYE7iu6D8t7fbOmi9KUL2zedwgF95iLwI.zAFSApwj14i", new DateTime(2025, 4, 15, 20, 9, 30, 242, DateTimeKind.Utc).AddTicks(9905) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 15, 20, 9, 30, 242, DateTimeKind.Utc).AddTicks(9830), "$2a$11$TxRzazTkfYE7iu6D8t7fbOmi9KUL2zedwgF95iLwI.zAFSApwj14i", new DateTime(2025, 4, 15, 20, 9, 30, 242, DateTimeKind.Utc).AddTicks(9830) });

            migrationBuilder.AddForeignKey(
                name: "FK_DecisionTemplate_Users_ApprovedByUserId",
                table: "DecisionTemplate",
                column: "ApprovedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DecisionTemplate_Users_ApprovedByUserId",
                table: "DecisionTemplate");

            migrationBuilder.AlterColumn<string>(
                name: "ApprovedByUserId",
                table: "DecisionTemplate",
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

            migrationBuilder.AddForeignKey(
                name: "FK_DecisionTemplate_Users_ApprovedByUserId",
                table: "DecisionTemplate",
                column: "ApprovedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
