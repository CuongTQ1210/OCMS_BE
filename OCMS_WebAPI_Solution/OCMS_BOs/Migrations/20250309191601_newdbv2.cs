using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class newdbv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_CertificatesTemplate_CertificateTemplateId",
                table: "Certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_CertificatesTemplate_Users_CreatedBy",
                table: "CertificatesTemplate");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Users_CreatedByUserUserId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Users_InstructorId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_ExternalCertificates_Users_UserId",
                table: "ExternalCertificates");

            migrationBuilder.DropForeignKey(
                name: "FK_Grades_Courses_CourseId",
                table: "Grades");

            migrationBuilder.DropForeignKey(
                name: "FK_Grades_Users_SubmittedBy",
                table: "Grades");

            migrationBuilder.DropForeignKey(
                name: "FK_Grades_Users_TraineeId",
                table: "Grades");

            migrationBuilder.DropTable(
                name: "CourseChangeRequests");

            migrationBuilder.DropTable(
                name: "TraineeNotifications");

            migrationBuilder.DropTable(
                name: "TraineeProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Grades_CourseId",
                table: "Grades");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CertificatesTemplate",
                table: "CertificatesTemplate");

            migrationBuilder.DropIndex(
                name: "IX_CertificatesTemplate_CreatedBy",
                table: "CertificatesTemplate");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Certificates");

            migrationBuilder.RenameTable(
                name: "CertificatesTemplate",
                newName: "CertificateTemplates");

            migrationBuilder.RenameColumn(
                name: "ScheduleFile",
                table: "Subjects",
                newName: "Schedule");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Grades",
                newName: "UpdateDate");

            migrationBuilder.RenameColumn(
                name: "TraineeId",
                table: "Grades",
                newName: "TraineeAssignID");

            migrationBuilder.RenameColumn(
                name: "SubmittedBy",
                table: "Grades",
                newName: "SubjectId");

            migrationBuilder.RenameColumn(
                name: "GradeValue",
                table: "Grades",
                newName: "gradeStatus");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "Grades",
                newName: "Remarks");

            migrationBuilder.RenameIndex(
                name: "IX_Grades_TraineeId",
                table: "Grades",
                newName: "IX_Grades_TraineeAssignID");

            migrationBuilder.RenameIndex(
                name: "IX_Grades_SubmittedBy",
                table: "Grades",
                newName: "IX_Grades_SubjectId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ExternalCertificates",
                newName: "VerifyByUserId");

            migrationBuilder.RenameColumn(
                name: "CertificateFile",
                table: "ExternalCertificates",
                newName: "PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_ExternalCertificates_UserId",
                table: "ExternalCertificates",
                newName: "IX_ExternalCertificates_VerifyByUserId");

            migrationBuilder.RenameColumn(
                name: "InstructorId",
                table: "Courses",
                newName: "TrainingPlanId");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserUserId",
                table: "Courses",
                newName: "ApproveByUserId");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Courses",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "CourseType",
                table: "Courses",
                newName: "CourseLevel");

            migrationBuilder.RenameIndex(
                name: "IX_Courses_InstructorId",
                table: "Courses",
                newName: "IX_Courses_TrainingPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_Courses_CreatedByUserUserId",
                table: "Courses",
                newName: "IX_Courses_ApproveByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Certificates",
                newName: "SignDate");

            migrationBuilder.RenameColumn(
                name: "DigitalSignature",
                table: "Certificates",
                newName: "IssueByUserId");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "CertificateTemplates",
                newName: "Description");

            migrationBuilder.AddColumn<string>(
                name: "CreateByUserId",
                table: "Subjects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Credits",
                table: "Subjects",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Subjects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "PassingScore",
                table: "Subjects",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "GradedByInstructorId",
                table: "Grades",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Score",
                table: "Grades",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "CertificateCode",
                table: "ExternalCertificates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CertificateFileURL",
                table: "ExternalCertificates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CertificateName",
                table: "ExternalCertificates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IssuingOrganization",
                table: "ExternalCertificates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "VerificationStatus",
                table: "ExternalCertificates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifyDate",
                table: "ExternalCertificates",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ApprovebyUserId",
                table: "Certificates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CertificateCode",
                table: "Certificates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CertificateURL",
                table: "Certificates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DigitalSignatureId",
                table: "Certificates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RevocationDate",
                table: "Certificates",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApprovedBy",
                table: "ApprovalLogs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                table: "CertificateTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreateByUserUserId",
                table: "CertificateTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "CertificateTemplates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedAt",
                table: "CertificateTemplates",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "templateStatus",
                table: "CertificateTemplates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CertificateTemplates",
                table: "CertificateTemplates",
                column: "CertificateTemplateId");

            migrationBuilder.CreateTable(
                name: "Candidate",
                columns: table => new
                {
                    CandidateId = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Gender = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    PersonalID = table.Column<string>(type: "text", nullable: false),
                    ExternalCertificate = table.Column<string>(type: "text", nullable: true),
                    CandidateStatus = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ImportByUserID = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidate", x => x.CandidateId);
                    table.ForeignKey(
                        name: "FK_Candidate_Users_ImportByUserID",
                        column: x => x.ImportByUserID,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseResults",
                columns: table => new
                {
                    ResultID = table.Column<string>(type: "text", nullable: false),
                    CourseID = table.Column<string>(type: "text", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TotalTrainees = table.Column<int>(type: "integer", nullable: false),
                    PassCount = table.Column<int>(type: "integer", nullable: false),
                    FailCount = table.Column<int>(type: "integer", nullable: false),
                    AverageScore = table.Column<double>(type: "double precision", nullable: false),
                    SubmittedBy = table.Column<string>(type: "text", nullable: false),
                    SubmittedByUserUserId = table.Column<string>(type: "text", nullable: true),
                    SubmissionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    ApprovedByUserUserId = table.Column<string>(type: "text", nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseResults", x => x.ResultID);
                    table.ForeignKey(
                        name: "FK_CourseResults_Courses_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseResults_Users_ApprovedByUserUserId",
                        column: x => x.ApprovedByUserUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_CourseResults_Users_SubmittedByUserUserId",
                        column: x => x.SubmittedByUserUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentId = table.Column<string>(type: "text", nullable: false),
                    DepartmentName = table.Column<string>(type: "text", nullable: false),
                    DepartmentDescription = table.Column<string>(type: "text", nullable: false),
                    ManagerId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentId);
                    table.ForeignKey(
                        name: "FK_Departments_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DigitalSignatures",
                columns: table => new
                {
                    SignatureID = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    PublicKey = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    CertificateChain = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalSignatures", x => x.SignatureID);
                    table.ForeignKey(
                        name: "FK_DigitalSignatures_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InstructorAssignments",
                columns: table => new
                {
                    AssignmentId = table.Column<string>(type: "text", nullable: false),
                    SubjectId = table.Column<string>(type: "text", nullable: false),
                    InstructorId = table.Column<string>(type: "text", nullable: false),
                    AssignByUserId = table.Column<string>(type: "text", nullable: false),
                    AssignDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    RequestStatus = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstructorAssignments", x => x.AssignmentId);
                    table.ForeignKey(
                        name: "FK_InstructorAssignments_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstructorAssignments_Users_AssignByUserId",
                        column: x => x.AssignByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstructorAssignments_Users_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    NotificationType = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    ReportId = table.Column<string>(type: "text", nullable: false),
                    ReportName = table.Column<string>(type: "text", nullable: false),
                    ReportType = table.Column<int>(type: "integer", nullable: false),
                    GenerateByUserId = table.Column<string>(type: "text", nullable: false),
                    GenerateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Format = table.Column<string>(type: "text", nullable: false),
                    FileUrl = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_Reports_Users_GenerateByUserId",
                        column: x => x.GenerateByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    RequestId = table.Column<string>(type: "text", nullable: false),
                    RequestUserId = table.Column<string>(type: "text", nullable: false),
                    RequestEntityId = table.Column<string>(type: "text", nullable: true),
                    RequestType = table.Column<int>(type: "integer", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    ApprovedUserUserId = table.Column<string>(type: "text", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_Requests_Users_ApprovedUserUserId",
                        column: x => x.ApprovedUserUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_Requests_Users_RequestUserId",
                        column: x => x.RequestUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TraineeAssignments",
                columns: table => new
                {
                    TraineeAssignId = table.Column<string>(type: "text", nullable: false),
                    TraineeId = table.Column<string>(type: "text", nullable: false),
                    CourseId = table.Column<string>(type: "text", nullable: false),
                    AssignDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ApproveByUserId = table.Column<string>(type: "text", nullable: false),
                    ApprovalDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraineeAssignments", x => x.TraineeAssignId);
                    table.ForeignKey(
                        name: "FK_TraineeAssignments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TraineeAssignments_Users_ApproveByUserId",
                        column: x => x.ApproveByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TraineeAssignments_Users_TraineeId",
                        column: x => x.TraineeId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingLists",
                columns: table => new
                {
                    ListId = table.Column<string>(type: "text", nullable: false),
                    ListName = table.Column<string>(type: "text", nullable: false),
                    CreateByUserId = table.Column<string>(type: "text", nullable: false),
                    Purpose = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ApproveByUserId = table.Column<string>(type: "text", nullable: false),
                    ApproveDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingLists", x => x.ListId);
                    table.ForeignKey(
                        name: "FK_TrainingLists_Users_ApproveByUserId",
                        column: x => x.ApproveByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainingLists_Users_CreateByUserId",
                        column: x => x.CreateByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingPlans",
                columns: table => new
                {
                    PlanId = table.Column<string>(type: "text", nullable: false),
                    PlanName = table.Column<string>(type: "text", nullable: false),
                    Desciption = table.Column<string>(type: "text", nullable: false),
                    PlanLevel = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreateByUserId = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ModifyDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ApproveByUserId = table.Column<string>(type: "text", nullable: true),
                    ApproveDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TrainingPlanStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingPlans", x => x.PlanId);
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

            migrationBuilder.CreateTable(
                name: "TrainingSchedules",
                columns: table => new
                {
                    ScheduleID = table.Column<string>(type: "text", nullable: false),
                    SubjectID = table.Column<string>(type: "text", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    Room = table.Column<string>(type: "text", nullable: false),
                    InstructorID = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedByUserUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingSchedules", x => x.ScheduleID);
                    table.ForeignKey(
                        name: "FK_TrainingSchedules_Subjects_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainingSchedules_Users_CreatedByUserUserId",
                        column: x => x.CreatedByUserUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_TrainingSchedules_Users_InstructorID",
                        column: x => x.InstructorID,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    ProfileId = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Gender = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    ExternalCertificate = table.Column<string>(type: "text", nullable: true),
                    DepartmentId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.ProfileId);
                    table.ForeignKey(
                        name: "FK_Profiles_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId");
                    table.ForeignKey(
                        name: "FK_Profiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Decisions",
                columns: table => new
                {
                    DecisionId = table.Column<string>(type: "text", nullable: false),
                    DecisionCode = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IssueByUserId = table.Column<string>(type: "text", nullable: false),
                    CertificateId = table.Column<string>(type: "text", nullable: false),
                    DigitalSignatureId = table.Column<string>(type: "text", nullable: false),
                    SignDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DecisionStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Decisions", x => x.DecisionId);
                    table.ForeignKey(
                        name: "FK_Decisions_Certificates_CertificateId",
                        column: x => x.CertificateId,
                        principalTable: "Certificates",
                        principalColumn: "CertificateId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Decisions_DigitalSignatures_DigitalSignatureId",
                        column: x => x.DigitalSignatureId,
                        principalTable: "DigitalSignatures",
                        principalColumn: "SignatureID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingListDetails",
                columns: table => new
                {
                    ListDetailId = table.Column<string>(type: "text", nullable: false),
                    TrainingListId = table.Column<string>(type: "text", nullable: false),
                    PersonId = table.Column<string>(type: "text", nullable: false),
                    RequestStatus = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingListDetails", x => x.ListDetailId);
                    table.ForeignKey(
                        name: "FK_TrainingListDetails_TrainingLists_TrainingListId",
                        column: x => x.TrainingListId,
                        principalTable: "TrainingLists",
                        principalColumn: "ListId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainingListDetails_Users_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanChangeRequests",
                columns: table => new
                {
                    PlanRequestId = table.Column<string>(type: "text", nullable: false),
                    PlanId = table.Column<string>(type: "text", nullable: false),
                    RequestedBy = table.Column<string>(type: "text", nullable: false),
                    RequestedUserUserId = table.Column<string>(type: "text", nullable: true),
                    RequestType = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ChangeDetails = table.Column<string>(type: "text", nullable: false),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    ApprovedUserUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanChangeRequests", x => x.PlanRequestId);
                    table.ForeignKey(
                        name: "FK_PlanChangeRequests_TrainingPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "TrainingPlans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanChangeRequests_Users_ApprovedUserUserId",
                        column: x => x.ApprovedUserUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_PlanChangeRequests_Users_RequestedUserUserId",
                        column: x => x.RequestedUserUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "RoleName" },
                values: new object[] { 8, "AOC Manager" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 9, 19, 16, 0, 544, DateTimeKind.Utc).AddTicks(9491), "$2a$11$fF2mblsNa9L/gD.aLhxCfOiGxZ/lq7ub256YAbjS4jp54YMG4Wgeu", new DateTime(2025, 3, 9, 19, 16, 0, 544, DateTimeKind.Utc).AddTicks(9487) });

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_CreateByUserId",
                table: "Subjects",
                column: "CreateByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Grades_GradedByInstructorId",
                table: "Grades",
                column: "GradedByInstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalCertificates_PersonId",
                table: "ExternalCertificates",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CreatedByUserId",
                table: "Courses",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_ApprovebyUserId",
                table: "Certificates",
                column: "ApprovebyUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_DigitalSignatureId",
                table: "Certificates",
                column: "DigitalSignatureId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_IssueByUserId",
                table: "Certificates",
                column: "IssueByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificateTemplates_ApprovedByUserId",
                table: "CertificateTemplates",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificateTemplates_CreateByUserUserId",
                table: "CertificateTemplates",
                column: "CreateByUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidate_ImportByUserID",
                table: "Candidate",
                column: "ImportByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_CourseResults_ApprovedByUserUserId",
                table: "CourseResults",
                column: "ApprovedByUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseResults_CourseID",
                table: "CourseResults",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_CourseResults_SubmittedByUserUserId",
                table: "CourseResults",
                column: "SubmittedByUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Decisions_CertificateId",
                table: "Decisions",
                column: "CertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_Decisions_DigitalSignatureId",
                table: "Decisions",
                column: "DigitalSignatureId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ManagerId",
                table: "Departments",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalSignatures_UserId",
                table: "DigitalSignatures",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InstructorAssignments_AssignByUserId",
                table: "InstructorAssignments",
                column: "AssignByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InstructorAssignments_InstructorId",
                table: "InstructorAssignments",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_InstructorAssignments_SubjectId",
                table: "InstructorAssignments",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanChangeRequests_ApprovedUserUserId",
                table: "PlanChangeRequests",
                column: "ApprovedUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanChangeRequests_PlanId",
                table: "PlanChangeRequests",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanChangeRequests_RequestedUserUserId",
                table: "PlanChangeRequests",
                column: "RequestedUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_DepartmentId",
                table: "Profiles",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_UserId",
                table: "Profiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_GenerateByUserId",
                table: "Reports",
                column: "GenerateByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_ApprovedUserUserId",
                table: "Requests",
                column: "ApprovedUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_RequestUserId",
                table: "Requests",
                column: "RequestUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeAssignments_ApproveByUserId",
                table: "TraineeAssignments",
                column: "ApproveByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeAssignments_CourseId",
                table: "TraineeAssignments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeAssignments_TraineeId",
                table: "TraineeAssignments",
                column: "TraineeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingListDetails_PersonId",
                table: "TrainingListDetails",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingListDetails_TrainingListId",
                table: "TrainingListDetails",
                column: "TrainingListId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingLists_ApproveByUserId",
                table: "TrainingLists",
                column: "ApproveByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingLists_CreateByUserId",
                table: "TrainingLists",
                column: "CreateByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlans_ApproveByUserId",
                table: "TrainingPlans",
                column: "ApproveByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlans_CreateByUserId",
                table: "TrainingPlans",
                column: "CreateByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingSchedules_CreatedByUserUserId",
                table: "TrainingSchedules",
                column: "CreatedByUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingSchedules_InstructorID",
                table: "TrainingSchedules",
                column: "InstructorID");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingSchedules_SubjectID",
                table: "TrainingSchedules",
                column: "SubjectID");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_CertificateTemplates_CertificateTemplateId",
                table: "Certificates",
                column: "CertificateTemplateId",
                principalTable: "CertificateTemplates",
                principalColumn: "CertificateTemplateId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_DigitalSignatures_DigitalSignatureId",
                table: "Certificates",
                column: "DigitalSignatureId",
                principalTable: "DigitalSignatures",
                principalColumn: "SignatureID");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_Users_ApprovebyUserId",
                table: "Certificates",
                column: "ApprovebyUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_Users_IssueByUserId",
                table: "Certificates",
                column: "IssueByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CertificateTemplates_Users_ApprovedByUserId",
                table: "CertificateTemplates",
                column: "ApprovedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CertificateTemplates_Users_CreateByUserUserId",
                table: "CertificateTemplates",
                column: "CreateByUserUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_TrainingPlans_TrainingPlanId",
                table: "Courses",
                column: "TrainingPlanId",
                principalTable: "TrainingPlans",
                principalColumn: "PlanId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Users_ApproveByUserId",
                table: "Courses",
                column: "ApproveByUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Users_CreatedByUserId",
                table: "Courses",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalCertificates_Candidate_PersonId",
                table: "ExternalCertificates",
                column: "PersonId",
                principalTable: "Candidate",
                principalColumn: "CandidateId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalCertificates_Users_VerifyByUserId",
                table: "ExternalCertificates",
                column: "VerifyByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_Subjects_SubjectId",
                table: "Grades",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "SubjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_TraineeAssignments_TraineeAssignID",
                table: "Grades",
                column: "TraineeAssignID",
                principalTable: "TraineeAssignments",
                principalColumn: "TraineeAssignId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_Users_GradedByInstructorId",
                table: "Grades",
                column: "GradedByInstructorId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Users_CreateByUserId",
                table: "Subjects",
                column: "CreateByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_CertificateTemplates_CertificateTemplateId",
                table: "Certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_DigitalSignatures_DigitalSignatureId",
                table: "Certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_Users_ApprovebyUserId",
                table: "Certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_Users_IssueByUserId",
                table: "Certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_CertificateTemplates_Users_ApprovedByUserId",
                table: "CertificateTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_CertificateTemplates_Users_CreateByUserUserId",
                table: "CertificateTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_TrainingPlans_TrainingPlanId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Users_ApproveByUserId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Users_CreatedByUserId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_ExternalCertificates_Candidate_PersonId",
                table: "ExternalCertificates");

            migrationBuilder.DropForeignKey(
                name: "FK_ExternalCertificates_Users_VerifyByUserId",
                table: "ExternalCertificates");

            migrationBuilder.DropForeignKey(
                name: "FK_Grades_Subjects_SubjectId",
                table: "Grades");

            migrationBuilder.DropForeignKey(
                name: "FK_Grades_TraineeAssignments_TraineeAssignID",
                table: "Grades");

            migrationBuilder.DropForeignKey(
                name: "FK_Grades_Users_GradedByInstructorId",
                table: "Grades");

            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Users_CreateByUserId",
                table: "Subjects");

            migrationBuilder.DropTable(
                name: "Candidate");

            migrationBuilder.DropTable(
                name: "CourseResults");

            migrationBuilder.DropTable(
                name: "Decisions");

            migrationBuilder.DropTable(
                name: "InstructorAssignments");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PlanChangeRequests");

            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropTable(
                name: "TraineeAssignments");

            migrationBuilder.DropTable(
                name: "TrainingListDetails");

            migrationBuilder.DropTable(
                name: "TrainingSchedules");

            migrationBuilder.DropTable(
                name: "DigitalSignatures");

            migrationBuilder.DropTable(
                name: "TrainingPlans");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "TrainingLists");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_CreateByUserId",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Grades_GradedByInstructorId",
                table: "Grades");

            migrationBuilder.DropIndex(
                name: "IX_ExternalCertificates_PersonId",
                table: "ExternalCertificates");

            migrationBuilder.DropIndex(
                name: "IX_Courses_CreatedByUserId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Certificates_ApprovebyUserId",
                table: "Certificates");

            migrationBuilder.DropIndex(
                name: "IX_Certificates_DigitalSignatureId",
                table: "Certificates");

            migrationBuilder.DropIndex(
                name: "IX_Certificates_IssueByUserId",
                table: "Certificates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CertificateTemplates",
                table: "CertificateTemplates");

            migrationBuilder.DropIndex(
                name: "IX_CertificateTemplates_ApprovedByUserId",
                table: "CertificateTemplates");

            migrationBuilder.DropIndex(
                name: "IX_CertificateTemplates_CreateByUserUserId",
                table: "CertificateTemplates");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 8);

            migrationBuilder.DropColumn(
                name: "CreateByUserId",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "Credits",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "PassingScore",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "GradedByInstructorId",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "CertificateCode",
                table: "ExternalCertificates");

            migrationBuilder.DropColumn(
                name: "CertificateFileURL",
                table: "ExternalCertificates");

            migrationBuilder.DropColumn(
                name: "CertificateName",
                table: "ExternalCertificates");

            migrationBuilder.DropColumn(
                name: "IssuingOrganization",
                table: "ExternalCertificates");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                table: "ExternalCertificates");

            migrationBuilder.DropColumn(
                name: "VerifyDate",
                table: "ExternalCertificates");

            migrationBuilder.DropColumn(
                name: "ApprovebyUserId",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "CertificateCode",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "CertificateURL",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "DigitalSignatureId",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "RevocationDate",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "CertificateTemplates");

            migrationBuilder.DropColumn(
                name: "CreateByUserUserId",
                table: "CertificateTemplates");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "CertificateTemplates");

            migrationBuilder.DropColumn(
                name: "LastUpdatedAt",
                table: "CertificateTemplates");

            migrationBuilder.DropColumn(
                name: "templateStatus",
                table: "CertificateTemplates");

            migrationBuilder.RenameTable(
                name: "CertificateTemplates",
                newName: "CertificatesTemplate");

            migrationBuilder.RenameColumn(
                name: "Schedule",
                table: "Subjects",
                newName: "ScheduleFile");

            migrationBuilder.RenameColumn(
                name: "gradeStatus",
                table: "Grades",
                newName: "GradeValue");

            migrationBuilder.RenameColumn(
                name: "UpdateDate",
                table: "Grades",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "TraineeAssignID",
                table: "Grades",
                newName: "TraineeId");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "Grades",
                newName: "SubmittedBy");

            migrationBuilder.RenameColumn(
                name: "Remarks",
                table: "Grades",
                newName: "CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_Grades_TraineeAssignID",
                table: "Grades",
                newName: "IX_Grades_TraineeId");

            migrationBuilder.RenameIndex(
                name: "IX_Grades_SubjectId",
                table: "Grades",
                newName: "IX_Grades_SubmittedBy");

            migrationBuilder.RenameColumn(
                name: "VerifyByUserId",
                table: "ExternalCertificates",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "PersonId",
                table: "ExternalCertificates",
                newName: "CertificateFile");

            migrationBuilder.RenameIndex(
                name: "IX_ExternalCertificates_VerifyByUserId",
                table: "ExternalCertificates",
                newName: "IX_ExternalCertificates_UserId");

            migrationBuilder.RenameColumn(
                name: "TrainingPlanId",
                table: "Courses",
                newName: "InstructorId");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Courses",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "CourseLevel",
                table: "Courses",
                newName: "CourseType");

            migrationBuilder.RenameColumn(
                name: "ApproveByUserId",
                table: "Courses",
                newName: "CreatedByUserUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Courses_TrainingPlanId",
                table: "Courses",
                newName: "IX_Courses_InstructorId");

            migrationBuilder.RenameIndex(
                name: "IX_Courses_ApproveByUserId",
                table: "Courses",
                newName: "IX_Courses_CreatedByUserUserId");

            migrationBuilder.RenameColumn(
                name: "SignDate",
                table: "Certificates",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "IssueByUserId",
                table: "Certificates",
                newName: "DigitalSignature");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "CertificatesTemplate",
                newName: "CreatedBy");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Grades",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Certificates",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "ApprovedBy",
                table: "ApprovalLogs",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CertificatesTemplate",
                table: "CertificatesTemplate",
                column: "CertificateTemplateId");

            migrationBuilder.CreateTable(
                name: "CourseChangeRequests",
                columns: table => new
                {
                    ChangeRequestId = table.Column<string>(type: "text", nullable: false),
                    ApprovedUserUserId = table.Column<string>(type: "text", nullable: true),
                    CourseId = table.Column<string>(type: "text", nullable: false),
                    RequestedUserUserId = table.Column<string>(type: "text", nullable: true),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    ChangeDetails = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    RequestType = table.Column<string>(type: "text", nullable: false),
                    RequestedBy = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseChangeRequests", x => x.ChangeRequestId);
                    table.ForeignKey(
                        name: "FK_CourseChangeRequests_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseChangeRequests_Users_ApprovedUserUserId",
                        column: x => x.ApprovedUserUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_CourseChangeRequests_Users_RequestedUserUserId",
                        column: x => x.RequestedUserUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "TraineeNotifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    NotificationType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraineeNotifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_TraineeNotifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TraineeProfiles",
                columns: table => new
                {
                    TraineeProfileId = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    ExternalCertificate = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraineeProfiles", x => x.TraineeProfileId);
                    table.ForeignKey(
                        name: "FK_TraineeProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 2, 21, 8, 3, 11, 845, DateTimeKind.Utc).AddTicks(4676), "$2a$11$EoSDaFL7/q574IjgRlwWkOUjvS.m6indATZVGJSYgiFlVy9b432.6", new DateTime(2025, 2, 21, 8, 3, 11, 845, DateTimeKind.Utc).AddTicks(4674) });

            migrationBuilder.CreateIndex(
                name: "IX_Grades_CourseId",
                table: "Grades",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificatesTemplate_CreatedBy",
                table: "CertificatesTemplate",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CourseChangeRequests_ApprovedUserUserId",
                table: "CourseChangeRequests",
                column: "ApprovedUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseChangeRequests_CourseId",
                table: "CourseChangeRequests",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseChangeRequests_RequestedUserUserId",
                table: "CourseChangeRequests",
                column: "RequestedUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeNotifications_UserId",
                table: "TraineeNotifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeProfiles_UserId",
                table: "TraineeProfiles",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_CertificatesTemplate_CertificateTemplateId",
                table: "Certificates",
                column: "CertificateTemplateId",
                principalTable: "CertificatesTemplate",
                principalColumn: "CertificateTemplateId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CertificatesTemplate_Users_CreatedBy",
                table: "CertificatesTemplate",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Users_CreatedByUserUserId",
                table: "Courses",
                column: "CreatedByUserUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Users_InstructorId",
                table: "Courses",
                column: "InstructorId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalCertificates_Users_UserId",
                table: "ExternalCertificates",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_Courses_CourseId",
                table: "Grades",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_Users_SubmittedBy",
                table: "Grades",
                column: "SubmittedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_Users_TraineeId",
                table: "Grades",
                column: "TraineeId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
