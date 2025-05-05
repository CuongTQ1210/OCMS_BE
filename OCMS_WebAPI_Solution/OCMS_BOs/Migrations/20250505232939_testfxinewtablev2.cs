using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class testfxinewtablev2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseSubjectSpecialties_Courses_CourseId1",
                table: "CourseSubjectSpecialties");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSubjectSpecialties_Subjects_SubjectId1",
                table: "CourseSubjectSpecialties");

            migrationBuilder.DropIndex(
                name: "IX_CourseSubjectSpecialties_CourseId1",
                table: "CourseSubjectSpecialties");

            migrationBuilder.DropIndex(
                name: "IX_CourseSubjectSpecialties_SubjectId1",
                table: "CourseSubjectSpecialties");

            migrationBuilder.DropColumn(
                name: "CourseId1",
                table: "CourseSubjectSpecialties");

            migrationBuilder.DropColumn(
                name: "SubjectId1",
                table: "CourseSubjectSpecialties");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "CourseSubjectSpecialties",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "CourseSubjectSpecialties",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CourseSubjectSpecialties",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "CourseSubjectSpecialties",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "CourseSubjectSpecialties",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CourseSubjectSpecialties",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<string>(
                name: "CourseId1",
                table: "CourseSubjectSpecialties",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubjectId1",
                table: "CourseSubjectSpecialties",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 23, 25, 38, 98, DateTimeKind.Utc).AddTicks(9320), new DateTime(2025, 5, 6, 6, 25, 38, 98, DateTimeKind.Local).AddTicks(9318) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 23, 25, 38, 464, DateTimeKind.Utc).AddTicks(8566), "$2a$11$yO/mviTurs3KkKhRUgbMm.A1vq1vCvBYnybamzXti.DD/WwPJmfHC", new DateTime(2025, 5, 5, 23, 25, 38, 464, DateTimeKind.Utc).AddTicks(8566) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 23, 25, 38, 464, DateTimeKind.Utc).AddTicks(8590), "$2a$11$4OTAKFNPK.LqtH.COzxD1e/eGKEfnwE0/SMElP6ucUyISQKvAlg6O", new DateTime(2025, 5, 5, 23, 25, 38, 464, DateTimeKind.Utc).AddTicks(8590) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 23, 25, 38, 464, DateTimeKind.Utc).AddTicks(8572), "$2a$11$4OTAKFNPK.LqtH.COzxD1e/eGKEfnwE0/SMElP6ucUyISQKvAlg6O", new DateTime(2025, 5, 5, 23, 25, 38, 464, DateTimeKind.Utc).AddTicks(8573) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 23, 25, 38, 464, DateTimeKind.Utc).AddTicks(8579), "$2a$11$4OTAKFNPK.LqtH.COzxD1e/eGKEfnwE0/SMElP6ucUyISQKvAlg6O", new DateTime(2025, 5, 5, 23, 25, 38, 464, DateTimeKind.Utc).AddTicks(8579) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 23, 25, 38, 464, DateTimeKind.Utc).AddTicks(8582), "$2a$11$4OTAKFNPK.LqtH.COzxD1e/eGKEfnwE0/SMElP6ucUyISQKvAlg6O", new DateTime(2025, 5, 5, 23, 25, 38, 464, DateTimeKind.Utc).AddTicks(8582) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 23, 25, 38, 464, DateTimeKind.Utc).AddTicks(8584), "$2a$11$4OTAKFNPK.LqtH.COzxD1e/eGKEfnwE0/SMElP6ucUyISQKvAlg6O", new DateTime(2025, 5, 5, 23, 25, 38, 464, DateTimeKind.Utc).AddTicks(8585) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 23, 25, 38, 464, DateTimeKind.Utc).AddTicks(8587), "$2a$11$4OTAKFNPK.LqtH.COzxD1e/eGKEfnwE0/SMElP6ucUyISQKvAlg6O", new DateTime(2025, 5, 5, 23, 25, 38, 464, DateTimeKind.Utc).AddTicks(8588) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 23, 25, 38, 464, DateTimeKind.Utc).AddTicks(8576), "$2a$11$4OTAKFNPK.LqtH.COzxD1e/eGKEfnwE0/SMElP6ucUyISQKvAlg6O", new DateTime(2025, 5, 5, 23, 25, 38, 464, DateTimeKind.Utc).AddTicks(8576) });

            migrationBuilder.CreateIndex(
                name: "IX_CourseSubjectSpecialties_CourseId1",
                table: "CourseSubjectSpecialties",
                column: "CourseId1");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSubjectSpecialties_SubjectId1",
                table: "CourseSubjectSpecialties",
                column: "SubjectId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSubjectSpecialties_Courses_CourseId1",
                table: "CourseSubjectSpecialties",
                column: "CourseId1",
                principalTable: "Courses",
                principalColumn: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSubjectSpecialties_Subjects_SubjectId1",
                table: "CourseSubjectSpecialties",
                column: "SubjectId1",
                principalTable: "Subjects",
                principalColumn: "SubjectId");
        }
    }
}
