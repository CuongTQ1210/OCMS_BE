using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class newdbv11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Users_ApproveByUserId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_InstructorAssignments_CourseSubjectSpecialties_CourseSubjec~",
                table: "InstructorAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TraineeAssignments_CourseSubjectSpecialties_CourseSubjectSp~",
                table: "TraineeAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingSchedules_CourseSubjectSpecialties_CourseSubjectSpe~",
                table: "TrainingSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingSchedules_TrainingPlans_TrainingPlanPlanId",
                table: "TrainingSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingSchedules_Users_InstructorID",
                table: "TrainingSchedules");

            migrationBuilder.DropTable(
                name: "CourseSubjectSpecialties");

            migrationBuilder.DropTable(
                name: "TrainingPlans");

            migrationBuilder.DropIndex(
                name: "IX_TrainingSchedules_CourseSubjectSpecialtyId",
                table: "TrainingSchedules");

            migrationBuilder.DropIndex(
                name: "IX_TrainingSchedules_TrainingPlanPlanId",
                table: "TrainingSchedules");

            migrationBuilder.DropIndex(
                name: "IX_Grades_TraineeAssignID",
                table: "Grades");

            migrationBuilder.DropIndex(
                name: "IX_Courses_ApproveByUserId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "CourseSubjectSpecialtyId",
                table: "TrainingSchedules");

            migrationBuilder.DropColumn(
                name: "TrainingPlanPlanId",
                table: "TrainingSchedules");

            migrationBuilder.DropColumn(
                name: "ApprovalDate",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "ApproveByUserId",
                table: "Courses");

            migrationBuilder.RenameColumn(
                name: "InstructorID",
                table: "TrainingSchedules",
                newName: "ClassSubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingSchedules_InstructorID",
                table: "TrainingSchedules",
                newName: "IX_TrainingSchedules_ClassSubjectId");

            migrationBuilder.RenameColumn(
                name: "CourseSubjectSpecialtyId",
                table: "TraineeAssignments",
                newName: "ClassSubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_TraineeAssignments_CourseSubjectSpecialtyId",
                table: "TraineeAssignments",
                newName: "IX_TraineeAssignments_ClassSubjectId");

            migrationBuilder.RenameColumn(
                name: "CourseSubjectSpecialtyId",
                table: "InstructorAssignments",
                newName: "SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_InstructorAssignments_CourseSubjectSpecialtyId",
                table: "InstructorAssignments",
                newName: "IX_InstructorAssignments_SubjectId");

            migrationBuilder.AlterColumn<string>(
                name: "SpecialtyId",
                table: "Users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDateTime",
                table: "Courses",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDateTime",
                table: "Courses",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Class",
                columns: table => new
                {
                    ClassId = table.Column<string>(type: "text", nullable: false),
                    ClassName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Class", x => x.ClassId);
                });

            migrationBuilder.CreateTable(
                name: "SubjectSpecialty",
                columns: table => new
                {
                    SubjectSpecialtyId = table.Column<string>(type: "text", nullable: false),
                    SpecialtyId = table.Column<string>(type: "text", nullable: false),
                    SubjectId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectSpecialty", x => x.SubjectSpecialtyId);
                    table.ForeignKey(
                        name: "FK_SubjectSpecialty_Specialties_SpecialtyId",
                        column: x => x.SpecialtyId,
                        principalTable: "Specialties",
                        principalColumn: "SpecialtyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubjectSpecialty_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassSubjects",
                columns: table => new
                {
                    ClassSubjectId = table.Column<string>(type: "text", nullable: false),
                    ClassId = table.Column<string>(type: "text", nullable: false),
                    SubjectId = table.Column<string>(type: "text", nullable: false),
                    InstructorAssignmentID = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassSubjects", x => x.ClassSubjectId);
                    table.ForeignKey(
                        name: "FK_ClassSubjects_Class_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Class",
                        principalColumn: "ClassId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassSubjects_InstructorAssignments_InstructorAssignmentID",
                        column: x => x.InstructorAssignmentID,
                        principalTable: "InstructorAssignments",
                        principalColumn: "AssignmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassSubjects_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseSubjectSpecialty",
                columns: table => new
                {
                    CoursesCourseId = table.Column<string>(type: "text", nullable: false),
                    SubjectSpecialtiesSubjectSpecialtyId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseSubjectSpecialty", x => new { x.CoursesCourseId, x.SubjectSpecialtiesSubjectSpecialtyId });
                    table.ForeignKey(
                        name: "FK_CourseSubjectSpecialty_Courses_CoursesCourseId",
                        column: x => x.CoursesCourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseSubjectSpecialty_SubjectSpecialty_SubjectSpecialtiesS~",
                        column: x => x.SubjectSpecialtiesSubjectSpecialtyId,
                        principalTable: "SubjectSpecialty",
                        principalColumn: "SubjectSpecialtyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 17, 10, 9, 43, 967, DateTimeKind.Utc).AddTicks(5228), new DateTime(2025, 5, 17, 17, 9, 43, 967, DateTimeKind.Local).AddTicks(5228) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3790), "$2a$11$uLNJyFekCqopEk.Rqr.JrOeseq39HI7dBix.XWsAP5VALdBhVtkKO", new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3791) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3813), "$2a$11$1LRxkdMsmJEdAkXMTCuLKO5cgNCNT4jVGq0w9oE6DHs.8HFPzeUrq", new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3813) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3794), "$2a$11$1LRxkdMsmJEdAkXMTCuLKO5cgNCNT4jVGq0w9oE6DHs.8HFPzeUrq", new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3795) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3802), "$2a$11$1LRxkdMsmJEdAkXMTCuLKO5cgNCNT4jVGq0w9oE6DHs.8HFPzeUrq", new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3802) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3805), "$2a$11$1LRxkdMsmJEdAkXMTCuLKO5cgNCNT4jVGq0w9oE6DHs.8HFPzeUrq", new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3805) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3808), "$2a$11$1LRxkdMsmJEdAkXMTCuLKO5cgNCNT4jVGq0w9oE6DHs.8HFPzeUrq", new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3808) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3810), "$2a$11$1LRxkdMsmJEdAkXMTCuLKO5cgNCNT4jVGq0w9oE6DHs.8HFPzeUrq", new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3811) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3797), "$2a$11$1LRxkdMsmJEdAkXMTCuLKO5cgNCNT4jVGq0w9oE6DHs.8HFPzeUrq", new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3798) });

            migrationBuilder.CreateIndex(
                name: "IX_Grades_TraineeAssignID",
                table: "Grades",
                column: "TraineeAssignID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassSubjects_ClassId",
                table: "ClassSubjects",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSubjects_InstructorAssignmentID",
                table: "ClassSubjects",
                column: "InstructorAssignmentID");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSubjects_SubjectId",
                table: "ClassSubjects",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSubjectSpecialty_SubjectSpecialtiesSubjectSpecialtyId",
                table: "CourseSubjectSpecialty",
                column: "SubjectSpecialtiesSubjectSpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectSpecialty_SpecialtyId",
                table: "SubjectSpecialty",
                column: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectSpecialty_SubjectId",
                table: "SubjectSpecialty",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_InstructorAssignments_Subjects_SubjectId",
                table: "InstructorAssignments",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "SubjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TraineeAssignments_ClassSubjects_ClassSubjectId",
                table: "TraineeAssignments",
                column: "ClassSubjectId",
                principalTable: "ClassSubjects",
                principalColumn: "ClassSubjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingSchedules_ClassSubjects_ClassSubjectId",
                table: "TrainingSchedules",
                column: "ClassSubjectId",
                principalTable: "ClassSubjects",
                principalColumn: "ClassSubjectId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InstructorAssignments_Subjects_SubjectId",
                table: "InstructorAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TraineeAssignments_ClassSubjects_ClassSubjectId",
                table: "TraineeAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingSchedules_ClassSubjects_ClassSubjectId",
                table: "TrainingSchedules");

            migrationBuilder.DropTable(
                name: "ClassSubjects");

            migrationBuilder.DropTable(
                name: "CourseSubjectSpecialty");

            migrationBuilder.DropTable(
                name: "Class");

            migrationBuilder.DropTable(
                name: "SubjectSpecialty");

            migrationBuilder.DropIndex(
                name: "IX_Grades_TraineeAssignID",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "EndDateTime",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "StartDateTime",
                table: "Courses");

            migrationBuilder.RenameColumn(
                name: "ClassSubjectId",
                table: "TrainingSchedules",
                newName: "InstructorID");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingSchedules_ClassSubjectId",
                table: "TrainingSchedules",
                newName: "IX_TrainingSchedules_InstructorID");

            migrationBuilder.RenameColumn(
                name: "ClassSubjectId",
                table: "TraineeAssignments",
                newName: "CourseSubjectSpecialtyId");

            migrationBuilder.RenameIndex(
                name: "IX_TraineeAssignments_ClassSubjectId",
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

            migrationBuilder.AlterColumn<string>(
                name: "SpecialtyId",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CourseSubjectSpecialtyId",
                table: "TrainingSchedules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TrainingPlanPlanId",
                table: "TrainingSchedules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovalDate",
                table: "Courses",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApproveByUserId",
                table: "Courses",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CourseSubjectSpecialties",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CourseId = table.Column<string>(type: "text", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: false),
                    SpecialtyId = table.Column<string>(type: "text", nullable: false),
                    SubjectId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseSubjectSpecialties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseSubjectSpecialties_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Restrict);
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
                        name: "FK_CourseSubjectSpecialties_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrainingPlans",
                columns: table => new
                {
                    PlanId = table.Column<string>(type: "text", nullable: false),
                    ApproveByUserId = table.Column<string>(type: "text", nullable: true),
                    CourseId = table.Column<string>(type: "text", nullable: false),
                    CreateByUserId = table.Column<string>(type: "text", nullable: false),
                    SpecialtyId = table.Column<string>(type: "text", nullable: false),
                    ApproveDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ModifyDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PlanName = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TrainingPlanStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingPlans", x => x.PlanId);
                    table.ForeignKey(
                        name: "FK_TrainingPlans_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainingPlans_Specialties_SpecialtyId",
                        column: x => x.SpecialtyId,
                        principalTable: "Specialties",
                        principalColumn: "SpecialtyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainingPlans_Users_ApproveByUserId",
                        column: x => x.ApproveByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_TrainingPlans_Users_CreateByUserId",
                        column: x => x.CreateByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 13, 53, 51, 244, DateTimeKind.Utc).AddTicks(6996), new DateTime(2025, 5, 6, 20, 53, 51, 244, DateTimeKind.Local).AddTicks(6994) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(91), "$2a$11$E7dtbJRZ2Q7sA.lqUamyQeKc2Ru8p/YKzETpAFwcJLsVwFl4fr4aW", new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(91) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(113), "$2a$11$3DtQlMoEhXY1bGb/Dymo/ewulyg0DOvE/a7ivCU85Rrea/SzBeKhS", new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(113) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(96), "$2a$11$3DtQlMoEhXY1bGb/Dymo/ewulyg0DOvE/a7ivCU85Rrea/SzBeKhS", new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(97) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(102), "$2a$11$3DtQlMoEhXY1bGb/Dymo/ewulyg0DOvE/a7ivCU85Rrea/SzBeKhS", new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(103) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(105), "$2a$11$3DtQlMoEhXY1bGb/Dymo/ewulyg0DOvE/a7ivCU85Rrea/SzBeKhS", new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(105) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(108), "$2a$11$3DtQlMoEhXY1bGb/Dymo/ewulyg0DOvE/a7ivCU85Rrea/SzBeKhS", new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(108) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(110), "$2a$11$3DtQlMoEhXY1bGb/Dymo/ewulyg0DOvE/a7ivCU85Rrea/SzBeKhS", new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(111) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(100), "$2a$11$3DtQlMoEhXY1bGb/Dymo/ewulyg0DOvE/a7ivCU85Rrea/SzBeKhS", new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(100) });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingSchedules_CourseSubjectSpecialtyId",
                table: "TrainingSchedules",
                column: "CourseSubjectSpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingSchedules_TrainingPlanPlanId",
                table: "TrainingSchedules",
                column: "TrainingPlanPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Grades_TraineeAssignID",
                table: "Grades",
                column: "TraineeAssignID");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_ApproveByUserId",
                table: "Courses",
                column: "ApproveByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSubjectSpecialties_CourseId",
                table: "CourseSubjectSpecialties",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSubjectSpecialties_CreatedByUserId",
                table: "CourseSubjectSpecialties",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSubjectSpecialties_SpecialtyId",
                table: "CourseSubjectSpecialties",
                column: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSubjectSpecialties_SubjectId",
                table: "CourseSubjectSpecialties",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlans_ApproveByUserId",
                table: "TrainingPlans",
                column: "ApproveByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlans_CourseId",
                table: "TrainingPlans",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlans_CreateByUserId",
                table: "TrainingPlans",
                column: "CreateByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlans_SpecialtyId",
                table: "TrainingPlans",
                column: "SpecialtyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Users_ApproveByUserId",
                table: "Courses",
                column: "ApproveByUserId",
                principalTable: "Users",
                principalColumn: "UserId");

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
                name: "FK_TrainingSchedules_CourseSubjectSpecialties_CourseSubjectSpe~",
                table: "TrainingSchedules",
                column: "CourseSubjectSpecialtyId",
                principalTable: "CourseSubjectSpecialties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingSchedules_TrainingPlans_TrainingPlanPlanId",
                table: "TrainingSchedules",
                column: "TrainingPlanPlanId",
                principalTable: "TrainingPlans",
                principalColumn: "PlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingSchedules_Users_InstructorID",
                table: "TrainingSchedules",
                column: "InstructorID",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
