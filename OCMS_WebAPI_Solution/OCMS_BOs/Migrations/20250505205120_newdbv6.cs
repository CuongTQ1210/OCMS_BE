using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class newdbv6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grades_Subjects_SubjectId",
                table: "Grades");

            migrationBuilder.DropForeignKey(
                name: "FK_InstructorAssignments_Subjects_SubjectId",
                table: "InstructorAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Courses_CourseId",
                table: "Subjects");

            migrationBuilder.DropForeignKey(
                name: "FK_TraineeAssignments_Courses_CourseId",
                table: "TraineeAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingPlans_Specialties_SpecialtyId",
                table: "TrainingPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingSchedules_Subjects_SubjectID",
                table: "TrainingSchedules");

            migrationBuilder.DropIndex(
                name: "IX_TrainingPlans_SpecialtyId",
                table: "TrainingPlans");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_CourseId",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Grades_SubjectId",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "PlanLevel",
                table: "TrainingPlans");

            migrationBuilder.DropColumn(
                name: "SpecialtyId",
                table: "TrainingPlans");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "Grades");

            migrationBuilder.RenameColumn(
                name: "SubjectID",
                table: "TrainingSchedules",
                newName: "CourseSubjectSpecialtyId");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingSchedules_SubjectID",
                table: "TrainingSchedules",
                newName: "IX_TrainingSchedules_CourseSubjectSpecialtyId");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "TraineeAssignments",
                newName: "CourseSubjectSpecialtyId");

            migrationBuilder.RenameIndex(
                name: "IX_TraineeAssignments_CourseId",
                table: "TraineeAssignments",
                newName: "IX_TraineeAssignments_CourseSubjectSpecialtyId");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "InstructorAssignments",
                newName: "CourseSubjectSpecialtyId");

            migrationBuilder.RenameIndex(
                name: "IX_InstructorAssignments_SubjectId",
                table: "InstructorAssignments",
                newName: "IX_InstructorAssignments_CourseSubjectSpecialtyId");

            migrationBuilder.AddColumn<string>(
                name: "SpecialtiesSpecialtyId",
                table: "TrainingPlans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IncludesRelearn",
                table: "Certificates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RelearnSubjects",
                table: "Certificates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialtyId",
                table: "Certificates",
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

            migrationBuilder.CreateTable(
                name: "CourseSubjectSpecialties",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CourseId = table.Column<string>(type: "text", nullable: false),
                    SubjectId = table.Column<string>(type: "text", nullable: false),
                    SpecialtyId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CertificateId = table.Column<string>(type: "text", nullable: true),
                    CourseId1 = table.Column<string>(type: "text", nullable: true),
                    SpecialtiesSpecialtyId = table.Column<string>(type: "text", nullable: true),
                    SubjectId1 = table.Column<string>(type: "text", nullable: true),
                    TrainingScheduleScheduleID = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseSubjectSpecialties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseSubjectSpecialties_Certificates_CertificateId",
                        column: x => x.CertificateId,
                        principalTable: "Certificates",
                        principalColumn: "CertificateId");
                    table.ForeignKey(
                        name: "FK_CourseSubjectSpecialties_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseSubjectSpecialties_Courses_CourseId1",
                        column: x => x.CourseId1,
                        principalTable: "Courses",
                        principalColumn: "CourseId");
                    table.ForeignKey(
                        name: "FK_CourseSubjectSpecialties_Specialties_SpecialtiesSpecialtyId",
                        column: x => x.SpecialtiesSpecialtyId,
                        principalTable: "Specialties",
                        principalColumn: "SpecialtyId");
                    table.ForeignKey(
                        name: "FK_CourseSubjectSpecialties_Specialties_SpecialtyId",
                        column: x => x.SpecialtyId,
                        principalTable: "Specialties",
                        principalColumn: "SpecialtyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseSubjectSpecialties_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseSubjectSpecialties_Subjects_SubjectId1",
                        column: x => x.SubjectId1,
                        principalTable: "Subjects",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_CourseSubjectSpecialties_TrainingSchedules_TrainingSchedule~",
                        column: x => x.TrainingScheduleScheduleID,
                        principalTable: "TrainingSchedules",
                        principalColumn: "ScheduleID");
                    table.ForeignKey(
                        name: "FK_CourseSubjectSpecialties_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 20, 51, 20, 179, DateTimeKind.Utc).AddTicks(3152), new DateTime(2025, 5, 6, 3, 51, 20, 179, DateTimeKind.Local).AddTicks(3150) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(243), "$2a$11$rDE0fJ1ZjXiwql9Clvb64OgvyilL7KL2MkN96MOIPwGaZzhXm.h0S", new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(244) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(268), "$2a$11$gyKYV.ZUPn0vPAcZYqQuru2di/PysSiNySC6nKiaNzlwdvN2gl29S", new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(268) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(251), "$2a$11$gyKYV.ZUPn0vPAcZYqQuru2di/PysSiNySC6nKiaNzlwdvN2gl29S", new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(251) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(257), "$2a$11$gyKYV.ZUPn0vPAcZYqQuru2di/PysSiNySC6nKiaNzlwdvN2gl29S", new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(257) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(259), "$2a$11$gyKYV.ZUPn0vPAcZYqQuru2di/PysSiNySC6nKiaNzlwdvN2gl29S", new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(260) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(262), "$2a$11$gyKYV.ZUPn0vPAcZYqQuru2di/PysSiNySC6nKiaNzlwdvN2gl29S", new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(263) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(265), "$2a$11$gyKYV.ZUPn0vPAcZYqQuru2di/PysSiNySC6nKiaNzlwdvN2gl29S", new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(265) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(254), "$2a$11$gyKYV.ZUPn0vPAcZYqQuru2di/PysSiNySC6nKiaNzlwdvN2gl29S", new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(254) });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlans_SpecialtiesSpecialtyId",
                table: "TrainingPlans",
                column: "SpecialtiesSpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_SpecialtyId",
                table: "Certificates",
                column: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSubject_SubjectsSubjectId",
                table: "CourseSubject",
                column: "SubjectsSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSubjectSpecialties_CertificateId",
                table: "CourseSubjectSpecialties",
                column: "CertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSubjectSpecialties_CourseId",
                table: "CourseSubjectSpecialties",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSubjectSpecialties_CourseId1",
                table: "CourseSubjectSpecialties",
                column: "CourseId1");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSubjectSpecialties_CreatedByUserId",
                table: "CourseSubjectSpecialties",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSubjectSpecialties_SpecialtiesSpecialtyId",
                table: "CourseSubjectSpecialties",
                column: "SpecialtiesSpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSubjectSpecialties_SpecialtyId",
                table: "CourseSubjectSpecialties",
                column: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSubjectSpecialties_SubjectId",
                table: "CourseSubjectSpecialties",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSubjectSpecialties_SubjectId1",
                table: "CourseSubjectSpecialties",
                column: "SubjectId1");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSubjectSpecialties_TrainingScheduleScheduleID",
                table: "CourseSubjectSpecialties",
                column: "TrainingScheduleScheduleID");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_Specialties_SpecialtyId",
                table: "Certificates",
                column: "SpecialtyId",
                principalTable: "Specialties",
                principalColumn: "SpecialtyId");

            migrationBuilder.AddForeignKey(
                name: "FK_InstructorAssignments_CourseSubjectSpecialties_CourseSubjec~",
                table: "InstructorAssignments",
                column: "CourseSubjectSpecialtyId",
                principalTable: "CourseSubjectSpecialties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TraineeAssignments_CourseSubjectSpecialties_CourseSubjectSp~",
                table: "TraineeAssignments",
                column: "CourseSubjectSpecialtyId",
                principalTable: "CourseSubjectSpecialties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingPlans_Specialties_SpecialtiesSpecialtyId",
                table: "TrainingPlans",
                column: "SpecialtiesSpecialtyId",
                principalTable: "Specialties",
                principalColumn: "SpecialtyId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingSchedules_CourseSubjectSpecialties_CourseSubjectSpe~",
                table: "TrainingSchedules",
                column: "CourseSubjectSpecialtyId",
                principalTable: "CourseSubjectSpecialties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_Specialties_SpecialtyId",
                table: "Certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_InstructorAssignments_CourseSubjectSpecialties_CourseSubjec~",
                table: "InstructorAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TraineeAssignments_CourseSubjectSpecialties_CourseSubjectSp~",
                table: "TraineeAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingPlans_Specialties_SpecialtiesSpecialtyId",
                table: "TrainingPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingSchedules_CourseSubjectSpecialties_CourseSubjectSpe~",
                table: "TrainingSchedules");

            migrationBuilder.DropTable(
                name: "CourseSubject");

            migrationBuilder.DropTable(
                name: "CourseSubjectSpecialties");

            migrationBuilder.DropIndex(
                name: "IX_TrainingPlans_SpecialtiesSpecialtyId",
                table: "TrainingPlans");

            migrationBuilder.DropIndex(
                name: "IX_Certificates_SpecialtyId",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "SpecialtiesSpecialtyId",
                table: "TrainingPlans");

            migrationBuilder.DropColumn(
                name: "IncludesRelearn",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "RelearnSubjects",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "SpecialtyId",
                table: "Certificates");

            migrationBuilder.RenameColumn(
                name: "CourseSubjectSpecialtyId",
                table: "TrainingSchedules",
                newName: "SubjectID");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingSchedules_CourseSubjectSpecialtyId",
                table: "TrainingSchedules",
                newName: "IX_TrainingSchedules_SubjectID");

            migrationBuilder.RenameColumn(
                name: "CourseSubjectSpecialtyId",
                table: "TraineeAssignments",
                newName: "CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_TraineeAssignments_CourseSubjectSpecialtyId",
                table: "TraineeAssignments",
                newName: "IX_TraineeAssignments_CourseId");

            migrationBuilder.RenameColumn(
                name: "CourseSubjectSpecialtyId",
                table: "InstructorAssignments",
                newName: "SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_InstructorAssignments_CourseSubjectSpecialtyId",
                table: "InstructorAssignments",
                newName: "IX_InstructorAssignments_SubjectId");

            migrationBuilder.AddColumn<int>(
                name: "PlanLevel",
                table: "TrainingPlans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SpecialtyId",
                table: "TrainingPlans",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CourseId",
                table: "Subjects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubjectId",
                table: "Grades",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 26, 17, 0, 1, 637, DateTimeKind.Utc).AddTicks(1610), new DateTime(2025, 4, 27, 0, 0, 1, 637, DateTimeKind.Local).AddTicks(1607) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3536), "$2a$11$tBJPPC0f3tVEc200l1o9SuvlfuH.snKX46yI5unXsXV2LmyytS7o6", new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3536) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3564), "$2a$11$4dAIhexXqTkA9/bemOPKWu898ATd/tukt97N9.JVmiSKyy.SV7JWC", new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3564) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3543), "$2a$11$4dAIhexXqTkA9/bemOPKWu898ATd/tukt97N9.JVmiSKyy.SV7JWC", new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3544) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3550), "$2a$11$4dAIhexXqTkA9/bemOPKWu898ATd/tukt97N9.JVmiSKyy.SV7JWC", new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3551) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3553), "$2a$11$4dAIhexXqTkA9/bemOPKWu898ATd/tukt97N9.JVmiSKyy.SV7JWC", new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3554) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3557), "$2a$11$4dAIhexXqTkA9/bemOPKWu898ATd/tukt97N9.JVmiSKyy.SV7JWC", new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3557) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3560), "$2a$11$4dAIhexXqTkA9/bemOPKWu898ATd/tukt97N9.JVmiSKyy.SV7JWC", new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3561) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3547), "$2a$11$4dAIhexXqTkA9/bemOPKWu898ATd/tukt97N9.JVmiSKyy.SV7JWC", new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3547) });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlans_SpecialtyId",
                table: "TrainingPlans",
                column: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_CourseId",
                table: "Subjects",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Grades_SubjectId",
                table: "Grades",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_Subjects_SubjectId",
                table: "Grades",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "SubjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InstructorAssignments_Subjects_SubjectId",
                table: "InstructorAssignments",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "SubjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Courses_CourseId",
                table: "Subjects",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TraineeAssignments_Courses_CourseId",
                table: "TraineeAssignments",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingPlans_Specialties_SpecialtyId",
                table: "TrainingPlans",
                column: "SpecialtyId",
                principalTable: "Specialties",
                principalColumn: "SpecialtyId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingSchedules_Subjects_SubjectID",
                table: "TrainingSchedules",
                column: "SubjectID",
                principalTable: "Subjects",
                principalColumn: "SubjectId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
