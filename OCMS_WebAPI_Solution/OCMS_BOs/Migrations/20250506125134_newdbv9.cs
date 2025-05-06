using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class newdbv9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 12, 51, 33, 99, DateTimeKind.Utc).AddTicks(6157), new DateTime(2025, 5, 6, 19, 51, 33, 99, DateTimeKind.Local).AddTicks(6155) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9704), "$2a$11$MVsN0XhrYTBEAERFcgIFXuCMSw62i0sM4GSzXf5h/3jQXGH5ErJ3W", new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9704) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9724), "$2a$11$VvwPivmzY96Y1QOunSVbkeCmGJj1f/xJhPAmtUc5iJk5iopgZ4pHu", new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9724) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9709), "$2a$11$VvwPivmzY96Y1QOunSVbkeCmGJj1f/xJhPAmtUc5iJk5iopgZ4pHu", new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9709) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9715), "$2a$11$VvwPivmzY96Y1QOunSVbkeCmGJj1f/xJhPAmtUc5iJk5iopgZ4pHu", new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9715) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9717), "$2a$11$VvwPivmzY96Y1QOunSVbkeCmGJj1f/xJhPAmtUc5iJk5iopgZ4pHu", new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9717) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9719), "$2a$11$VvwPivmzY96Y1QOunSVbkeCmGJj1f/xJhPAmtUc5iJk5iopgZ4pHu", new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9720) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9722), "$2a$11$VvwPivmzY96Y1QOunSVbkeCmGJj1f/xJhPAmtUc5iJk5iopgZ4pHu", new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9722) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9711), "$2a$11$VvwPivmzY96Y1QOunSVbkeCmGJj1f/xJhPAmtUc5iJk5iopgZ4pHu", new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9712) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 23, 29, 38, 304, DateTimeKind.Utc).AddTicks(3341), new DateTime(2025, 5, 6, 6, 29, 38, 304, DateTimeKind.Local).AddTicks(3339) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 23, 29, 38, 672, DateTimeKind.Utc).AddTicks(7618), "$2a$11$pMHqGBRAKecnau4ILgqyl.ClPMOmJjonMwDmcY8P8LeZhQN30anAG", new DateTime(2025, 5, 5, 23, 29, 38, 672, DateTimeKind.Utc).AddTicks(7618) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 23, 29, 38, 672, DateTimeKind.Utc).AddTicks(7642), "$2a$11$vNPgt80vHiXuuub6wlJu4O4bxLS.niVbBx2OsiJSa.IlqiiTKajlS", new DateTime(2025, 5, 5, 23, 29, 38, 672, DateTimeKind.Utc).AddTicks(7642) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 23, 29, 38, 672, DateTimeKind.Utc).AddTicks(7624), "$2a$11$vNPgt80vHiXuuub6wlJu4O4bxLS.niVbBx2OsiJSa.IlqiiTKajlS", new DateTime(2025, 5, 5, 23, 29, 38, 672, DateTimeKind.Utc).AddTicks(7624) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 23, 29, 38, 672, DateTimeKind.Utc).AddTicks(7630), "$2a$11$vNPgt80vHiXuuub6wlJu4O4bxLS.niVbBx2OsiJSa.IlqiiTKajlS", new DateTime(2025, 5, 5, 23, 29, 38, 672, DateTimeKind.Utc).AddTicks(7630) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 23, 29, 38, 672, DateTimeKind.Utc).AddTicks(7633), "$2a$11$vNPgt80vHiXuuub6wlJu4O4bxLS.niVbBx2OsiJSa.IlqiiTKajlS", new DateTime(2025, 5, 5, 23, 29, 38, 672, DateTimeKind.Utc).AddTicks(7633) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 23, 29, 38, 672, DateTimeKind.Utc).AddTicks(7636), "$2a$11$vNPgt80vHiXuuub6wlJu4O4bxLS.niVbBx2OsiJSa.IlqiiTKajlS", new DateTime(2025, 5, 5, 23, 29, 38, 672, DateTimeKind.Utc).AddTicks(7636) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 23, 29, 38, 672, DateTimeKind.Utc).AddTicks(7639), "$2a$11$vNPgt80vHiXuuub6wlJu4O4bxLS.niVbBx2OsiJSa.IlqiiTKajlS", new DateTime(2025, 5, 5, 23, 29, 38, 672, DateTimeKind.Utc).AddTicks(7639) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 23, 29, 38, 672, DateTimeKind.Utc).AddTicks(7627), "$2a$11$vNPgt80vHiXuuub6wlJu4O4bxLS.niVbBx2OsiJSa.IlqiiTKajlS", new DateTime(2025, 5, 5, 23, 29, 38, 672, DateTimeKind.Utc).AddTicks(7627) });
        }
    }
}
