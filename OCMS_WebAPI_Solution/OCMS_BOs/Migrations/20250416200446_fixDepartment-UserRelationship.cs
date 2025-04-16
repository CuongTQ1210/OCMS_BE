using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class fixDepartmentUserRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_DepartmentId",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "ManagerUserId",
                table: "Departments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 16, 20, 4, 45, 324, DateTimeKind.Utc).AddTicks(7979), new DateTime(2025, 4, 17, 3, 4, 45, 324, DateTimeKind.Local).AddTicks(7977) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(993), "$2a$11$dFDSSItCyxL9dZohK7lHzuSIWaXQ6oVqCDZlEwACaulyzQ0KPzgSi", new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(993) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1017), "$2a$11$npAeEcyD.zzUy5TRaWBFUudPyaWBaOSBhLwI3uz/0n1YYCCCjzSNe", new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1017) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1000), "$2a$11$npAeEcyD.zzUy5TRaWBFUudPyaWBaOSBhLwI3uz/0n1YYCCCjzSNe", new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1000) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1006), "$2a$11$npAeEcyD.zzUy5TRaWBFUudPyaWBaOSBhLwI3uz/0n1YYCCCjzSNe", new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1006) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1009), "$2a$11$npAeEcyD.zzUy5TRaWBFUudPyaWBaOSBhLwI3uz/0n1YYCCCjzSNe", new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1009) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1011), "$2a$11$npAeEcyD.zzUy5TRaWBFUudPyaWBaOSBhLwI3uz/0n1YYCCCjzSNe", new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1012) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1014), "$2a$11$npAeEcyD.zzUy5TRaWBFUudPyaWBaOSBhLwI3uz/0n1YYCCCjzSNe", new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1014) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1003), "$2a$11$npAeEcyD.zzUy5TRaWBFUudPyaWBaOSBhLwI3uz/0n1YYCCCjzSNe", new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1003) });

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentId",
                table: "Users",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ManagerUserId",
                table: "Departments",
                column: "ManagerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Users_ManagerUserId",
                table: "Departments",
                column: "ManagerUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Users_ManagerUserId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_DepartmentId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Departments_ManagerUserId",
                table: "Departments");

            migrationBuilder.AlterColumn<string>(
                name: "ManagerUserId",
                table: "Departments",
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

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentId",
                table: "Users",
                column: "DepartmentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId");
        }
    }
}
