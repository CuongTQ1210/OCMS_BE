using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class testfxinewtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseSubjectSpecialties_Certificates_CertificateId",
                table: "CourseSubjectSpecialties");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSubjectSpecialties_Specialties_SpecialtiesSpecialtyId",
                table: "CourseSubjectSpecialties");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSubjectSpecialties_TrainingSchedules_TrainingSchedule~",
                table: "CourseSubjectSpecialties");

            migrationBuilder.DropForeignKey(
                name: "FK_Specialties_Specialties_ParentSpecialtyId",
                table: "Specialties");

            migrationBuilder.DropForeignKey(
                name: "FK_Specialties_Users_CreatedByUserId",
                table: "Specialties");

            migrationBuilder.DropForeignKey(
                name: "FK_Specialties_Users_UpdatedByUserId",
                table: "Specialties");

            migrationBuilder.DropTable(
                name: "CourseSubject");

            migrationBuilder.DropIndex(
                name: "IX_CourseSubjectSpecialties_CertificateId",
                table: "CourseSubjectSpecialties");

            migrationBuilder.DropIndex(
                name: "IX_CourseSubjectSpecialties_SpecialtiesSpecialtyId",
                table: "CourseSubjectSpecialties");

            migrationBuilder.DropIndex(
                name: "IX_CourseSubjectSpecialties_TrainingScheduleScheduleID",
                table: "CourseSubjectSpecialties");

            migrationBuilder.DropColumn(
                name: "CertificateId",
                table: "CourseSubjectSpecialties");

            migrationBuilder.DropColumn(
                name: "SpecialtiesSpecialtyId",
                table: "CourseSubjectSpecialties");

            migrationBuilder.DropColumn(
                name: "TrainingScheduleScheduleID",
                table: "CourseSubjectSpecialties");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Specialties",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Specialties",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Specialties",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Specialties_Specialties_ParentSpecialtyId",
                table: "Specialties",
                column: "ParentSpecialtyId",
                principalTable: "Specialties",
                principalColumn: "SpecialtyId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Specialties_Users_CreatedByUserId",
                table: "Specialties",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Specialties_Users_UpdatedByUserId",
                table: "Specialties",
                column: "UpdatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Specialties_Specialties_ParentSpecialtyId",
                table: "Specialties");

            migrationBuilder.DropForeignKey(
                name: "FK_Specialties_Users_CreatedByUserId",
                table: "Specialties");

            migrationBuilder.DropForeignKey(
                name: "FK_Specialties_Users_UpdatedByUserId",
                table: "Specialties");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Specialties",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Specialties",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Specialties",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "CertificateId",
                table: "CourseSubjectSpecialties",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialtiesSpecialtyId",
                table: "CourseSubjectSpecialties",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrainingScheduleScheduleID",
                table: "CourseSubjectSpecialties",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CourseSubject",
                columns: table => new
                {
                    CoursesCourseId = table.Column<string>(type: "text", nullable: false),
                    SubjectsSubjectId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseSubject", x => new { x.CoursesCourseId, x.SubjectsSubjectId });
                    table.ForeignKey(
                        name: "FK_CourseSubject_Courses_CoursesCourseId",
                        column: x => x.CoursesCourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseSubject_Subjects_SubjectsSubjectId",
                        column: x => x.SubjectsSubjectId,
                        principalTable: "Subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 21, 42, 7, 655, DateTimeKind.Utc).AddTicks(4279), new DateTime(2025, 5, 6, 4, 42, 7, 655, DateTimeKind.Local).AddTicks(4277) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8920), "$2a$11$HEVa8fNy6Km157Y2gjs5R.ntTdJJpmG1Kn52qH/DHRC5VLq6TOXW6", new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8920) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8942), "$2a$11$RvWp9YTPNqnZAfs66WbABum7e9jbhqEi4zwdlhWSPOgnvyABBrK4a", new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8942) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8925), "$2a$11$RvWp9YTPNqnZAfs66WbABum7e9jbhqEi4zwdlhWSPOgnvyABBrK4a", new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8926) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8931), "$2a$11$RvWp9YTPNqnZAfs66WbABum7e9jbhqEi4zwdlhWSPOgnvyABBrK4a", new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8932) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8934), "$2a$11$RvWp9YTPNqnZAfs66WbABum7e9jbhqEi4zwdlhWSPOgnvyABBrK4a", new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8935) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8937), "$2a$11$RvWp9YTPNqnZAfs66WbABum7e9jbhqEi4zwdlhWSPOgnvyABBrK4a", new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8937) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8939), "$2a$11$RvWp9YTPNqnZAfs66WbABum7e9jbhqEi4zwdlhWSPOgnvyABBrK4a", new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8940) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8928), "$2a$11$RvWp9YTPNqnZAfs66WbABum7e9jbhqEi4zwdlhWSPOgnvyABBrK4a", new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8929) });

            migrationBuilder.CreateIndex(
                name: "IX_CourseSubjectSpecialties_CertificateId",
                table: "CourseSubjectSpecialties",
                column: "CertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSubjectSpecialties_SpecialtiesSpecialtyId",
                table: "CourseSubjectSpecialties",
                column: "SpecialtiesSpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSubjectSpecialties_TrainingScheduleScheduleID",
                table: "CourseSubjectSpecialties",
                column: "TrainingScheduleScheduleID");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSubject_SubjectsSubjectId",
                table: "CourseSubject",
                column: "SubjectsSubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSubjectSpecialties_Certificates_CertificateId",
                table: "CourseSubjectSpecialties",
                column: "CertificateId",
                principalTable: "Certificates",
                principalColumn: "CertificateId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSubjectSpecialties_Specialties_SpecialtiesSpecialtyId",
                table: "CourseSubjectSpecialties",
                column: "SpecialtiesSpecialtyId",
                principalTable: "Specialties",
                principalColumn: "SpecialtyId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSubjectSpecialties_TrainingSchedules_TrainingSchedule~",
                table: "CourseSubjectSpecialties",
                column: "TrainingScheduleScheduleID",
                principalTable: "TrainingSchedules",
                principalColumn: "ScheduleID");

            migrationBuilder.AddForeignKey(
                name: "FK_Specialties_Specialties_ParentSpecialtyId",
                table: "Specialties",
                column: "ParentSpecialtyId",
                principalTable: "Specialties",
                principalColumn: "SpecialtyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Specialties_Users_CreatedByUserId",
                table: "Specialties",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Specialties_Users_UpdatedByUserId",
                table: "Specialties",
                column: "UpdatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
