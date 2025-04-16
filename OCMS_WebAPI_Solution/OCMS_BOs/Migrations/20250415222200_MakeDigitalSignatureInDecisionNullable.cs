using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class MakeDigitalSignatureInDecisionNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Decisions_DigitalSignatures_DigitalSignatureId",
                table: "Decisions");

            migrationBuilder.AlterColumn<string>(
                name: "DigitalSignatureId",
                table: "Decisions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 15, 22, 21, 59, 938, DateTimeKind.Utc).AddTicks(4195), new DateTime(2025, 4, 16, 5, 21, 59, 938, DateTimeKind.Local).AddTicks(4194) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 15, 22, 22, 0, 187, DateTimeKind.Utc).AddTicks(7674), "$2a$11$wabE6CemV5.n.lHhCP/3tOyKr3.xrH1emQq9xJU/QRmxLxI7HvCaC", new DateTime(2025, 4, 15, 22, 22, 0, 187, DateTimeKind.Utc).AddTicks(7674) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 15, 22, 22, 0, 187, DateTimeKind.Utc).AddTicks(7712), "$2a$11$h0BlabfPWe/sV30.Un.87uauW/DQ9.fe8jseK2t3QK47t1OKX2LM2", new DateTime(2025, 4, 15, 22, 22, 0, 187, DateTimeKind.Utc).AddTicks(7712) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 15, 22, 22, 0, 187, DateTimeKind.Utc).AddTicks(7679), "$2a$11$h0BlabfPWe/sV30.Un.87uauW/DQ9.fe8jseK2t3QK47t1OKX2LM2", new DateTime(2025, 4, 15, 22, 22, 0, 187, DateTimeKind.Utc).AddTicks(7680) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 15, 22, 22, 0, 187, DateTimeKind.Utc).AddTicks(7686), "$2a$11$h0BlabfPWe/sV30.Un.87uauW/DQ9.fe8jseK2t3QK47t1OKX2LM2", new DateTime(2025, 4, 15, 22, 22, 0, 187, DateTimeKind.Utc).AddTicks(7686) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 15, 22, 22, 0, 187, DateTimeKind.Utc).AddTicks(7689), "$2a$11$h0BlabfPWe/sV30.Un.87uauW/DQ9.fe8jseK2t3QK47t1OKX2LM2", new DateTime(2025, 4, 15, 22, 22, 0, 187, DateTimeKind.Utc).AddTicks(7689) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 15, 22, 22, 0, 187, DateTimeKind.Utc).AddTicks(7692), "$2a$11$h0BlabfPWe/sV30.Un.87uauW/DQ9.fe8jseK2t3QK47t1OKX2LM2", new DateTime(2025, 4, 15, 22, 22, 0, 187, DateTimeKind.Utc).AddTicks(7692) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 15, 22, 22, 0, 187, DateTimeKind.Utc).AddTicks(7708), "$2a$11$h0BlabfPWe/sV30.Un.87uauW/DQ9.fe8jseK2t3QK47t1OKX2LM2", new DateTime(2025, 4, 15, 22, 22, 0, 187, DateTimeKind.Utc).AddTicks(7709) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 15, 22, 22, 0, 187, DateTimeKind.Utc).AddTicks(7682), "$2a$11$h0BlabfPWe/sV30.Un.87uauW/DQ9.fe8jseK2t3QK47t1OKX2LM2", new DateTime(2025, 4, 15, 22, 22, 0, 187, DateTimeKind.Utc).AddTicks(7683) });

            migrationBuilder.AddForeignKey(
                name: "FK_Decisions_DigitalSignatures_DigitalSignatureId",
                table: "Decisions",
                column: "DigitalSignatureId",
                principalTable: "DigitalSignatures",
                principalColumn: "SignatureID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Decisions_DigitalSignatures_DigitalSignatureId",
                table: "Decisions");

            migrationBuilder.AlterColumn<string>(
                name: "DigitalSignatureId",
                table: "Decisions",
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
                name: "FK_Decisions_DigitalSignatures_DigitalSignatureId",
                table: "Decisions",
                column: "DigitalSignatureId",
                principalTable: "DigitalSignatures",
                principalColumn: "SignatureID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
