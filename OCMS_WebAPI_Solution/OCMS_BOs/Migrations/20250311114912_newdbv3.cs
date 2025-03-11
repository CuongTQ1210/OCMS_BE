using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class newdbv3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Users_ManagerId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_ExternalCertificates_Candidate_PersonId",
                table: "ExternalCertificates");

            migrationBuilder.DropForeignKey(
                name: "FK_TraineeAssignments_Users_ApproveByUserId",
                table: "TraineeAssignments");

            migrationBuilder.DropTable(
                name: "ApprovalLogs");

            migrationBuilder.DropTable(
                name: "BackupLogs");

            migrationBuilder.DropTable(
                name: "PlanChangeRequests");

            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropTable(
                name: "TrainingListDetails");

            migrationBuilder.DropTable(
                name: "TrainingLists");

            migrationBuilder.DropIndex(
                name: "IX_ExternalCertificates_PersonId",
                table: "ExternalCertificates");

            migrationBuilder.DropIndex(
                name: "IX_Departments_ManagerId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "ExternalCertificates");

            migrationBuilder.RenameColumn(
                name: "ManagerId",
                table: "Departments",
                newName: "Specialty");

            migrationBuilder.RenameColumn(
                name: "IssueByUserId",
                table: "Decisions",
                newName: "IssuedByUserId");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Users",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DepartmentId",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ApproveByUserId",
                table: "TraineeAssignments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovalDate",
                table: "TraineeAssignments",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<string>(
                name: "CandidateId",
                table: "ExternalCertificates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ExternalCertificates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerUserId",
                table: "Departments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "DecisionCode",
                table: "Decisions",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "DecisionTemplateId",
                table: "Decisions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "DecisionTemplate",
                columns: table => new
                {
                    DecisionTemplateId = table.Column<string>(type: "text", nullable: false),
                    TemplateName = table.Column<string>(type: "text", nullable: false),
                    TemplateContent = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: false),
                    ApprovedByUserId = table.Column<string>(type: "text", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TemplateStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecisionTemplate", x => x.DecisionTemplateId);
                    table.ForeignKey(
                        name: "FK_DecisionTemplate_Users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DecisionTemplate_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "Address", "CreatedAt", "DateOfBirth", "DepartmentId", "FullName", "Gender", "PasswordHash", "PhoneNumber", "UpdatedAt" },
                values: new object[] { "123 Admin Street", new DateTime(2025, 3, 11, 11, 49, 11, 639, DateTimeKind.Utc).AddTicks(3115), new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Admin User", "Other", "$2a$11$FIyXTZCqUSFN66a3fQ19NuDBm3GxkSsWqE3L1Qpm3PQBq.EWW/NlK", "1234567890", new DateTime(2025, 3, 11, 11, 49, 11, 639, DateTimeKind.Utc).AddTicks(3116) });

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentId",
                table: "Users",
                column: "DepartmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalCertificates_CandidateId",
                table: "ExternalCertificates",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalCertificates_UserId",
                table: "ExternalCertificates",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Decisions_DecisionTemplateId",
                table: "Decisions",
                column: "DecisionTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Decisions_IssuedByUserId",
                table: "Decisions",
                column: "IssuedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DecisionTemplate_ApprovedByUserId",
                table: "DecisionTemplate",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DecisionTemplate_CreatedByUserId",
                table: "DecisionTemplate",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Decisions_DecisionTemplate_DecisionTemplateId",
                table: "Decisions",
                column: "DecisionTemplateId",
                principalTable: "DecisionTemplate",
                principalColumn: "DecisionTemplateId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Decisions_Users_IssuedByUserId",
                table: "Decisions",
                column: "IssuedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalCertificates_Candidate_CandidateId",
                table: "ExternalCertificates",
                column: "CandidateId",
                principalTable: "Candidate",
                principalColumn: "CandidateId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalCertificates_Users_UserId",
                table: "ExternalCertificates",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TraineeAssignments_Users_ApproveByUserId",
                table: "TraineeAssignments",
                column: "ApproveByUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Decisions_DecisionTemplate_DecisionTemplateId",
                table: "Decisions");

            migrationBuilder.DropForeignKey(
                name: "FK_Decisions_Users_IssuedByUserId",
                table: "Decisions");

            migrationBuilder.DropForeignKey(
                name: "FK_ExternalCertificates_Candidate_CandidateId",
                table: "ExternalCertificates");

            migrationBuilder.DropForeignKey(
                name: "FK_ExternalCertificates_Users_UserId",
                table: "ExternalCertificates");

            migrationBuilder.DropForeignKey(
                name: "FK_TraineeAssignments_Users_ApproveByUserId",
                table: "TraineeAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "DecisionTemplate");

            migrationBuilder.DropIndex(
                name: "IX_Users_DepartmentId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_ExternalCertificates_CandidateId",
                table: "ExternalCertificates");

            migrationBuilder.DropIndex(
                name: "IX_ExternalCertificates_UserId",
                table: "ExternalCertificates");

            migrationBuilder.DropIndex(
                name: "IX_Decisions_DecisionTemplateId",
                table: "Decisions");

            migrationBuilder.DropIndex(
                name: "IX_Decisions_IssuedByUserId",
                table: "Decisions");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CandidateId",
                table: "ExternalCertificates");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ExternalCertificates");

            migrationBuilder.DropColumn(
                name: "ManagerUserId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "DecisionTemplateId",
                table: "Decisions");

            migrationBuilder.RenameColumn(
                name: "Specialty",
                table: "Departments",
                newName: "ManagerId");

            migrationBuilder.RenameColumn(
                name: "IssuedByUserId",
                table: "Decisions",
                newName: "IssueByUserId");

            migrationBuilder.AlterColumn<string>(
                name: "ApproveByUserId",
                table: "TraineeAssignments",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovalDate",
                table: "TraineeAssignments",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonId",
                table: "ExternalCertificates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "DecisionCode",
                table: "Decisions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.CreateTable(
                name: "ApprovalLogs",
                columns: table => new
                {
                    ApprovalId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApprovedUserUserId = table.Column<string>(type: "text", nullable: true),
                    RequestedUserUserId = table.Column<string>(type: "text", nullable: true),
                    ActionDetails = table.Column<string>(type: "text", nullable: false),
                    ActionType = table.Column<string>(type: "text", nullable: false),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    RequestedBy = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalLogs", x => x.ApprovalId);
                    table.ForeignKey(
                        name: "FK_ApprovalLogs_Users_ApprovedUserUserId",
                        column: x => x.ApprovedUserUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_ApprovalLogs_Users_RequestedUserUserId",
                        column: x => x.RequestedUserUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "BackupLogs",
                columns: table => new
                {
                    BackupId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    BackupFilePath = table.Column<string>(type: "text", nullable: false),
                    BackupTimestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackupLogs", x => x.BackupId);
                    table.ForeignKey(
                        name: "FK_BackupLogs_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanChangeRequests",
                columns: table => new
                {
                    PlanRequestId = table.Column<string>(type: "text", nullable: false),
                    ApprovedUserUserId = table.Column<string>(type: "text", nullable: true),
                    PlanId = table.Column<string>(type: "text", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    ProfileId = table.Column<string>(type: "text", nullable: false),
                    DepartmentId = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    ExternalCertificate = table.Column<string>(type: "text", nullable: true),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Gender = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
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
                name: "TrainingLists",
                columns: table => new
                {
                    ListId = table.Column<string>(type: "text", nullable: false),
                    ApproveByUserId = table.Column<string>(type: "text", nullable: false),
                    CreateByUserId = table.Column<string>(type: "text", nullable: false),
                    ApproveDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ListName = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    Purpose = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
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
                name: "TrainingListDetails",
                columns: table => new
                {
                    ListDetailId = table.Column<string>(type: "text", nullable: false),
                    PersonId = table.Column<string>(type: "text", nullable: false),
                    TrainingListId = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    RequestStatus = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 9, 19, 16, 0, 544, DateTimeKind.Utc).AddTicks(9491), "$2a$11$fF2mblsNa9L/gD.aLhxCfOiGxZ/lq7ub256YAbjS4jp54YMG4Wgeu", new DateTime(2025, 3, 9, 19, 16, 0, 544, DateTimeKind.Utc).AddTicks(9487) });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalCertificates_PersonId",
                table: "ExternalCertificates",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ManagerId",
                table: "Departments",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalLogs_ApprovedUserUserId",
                table: "ApprovalLogs",
                column: "ApprovedUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalLogs_RequestedUserUserId",
                table: "ApprovalLogs",
                column: "RequestedUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BackupLogs_CreatedBy",
                table: "BackupLogs",
                column: "CreatedBy");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Users_ManagerId",
                table: "Departments",
                column: "ManagerId",
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
                name: "FK_TraineeAssignments_Users_ApproveByUserId",
                table: "TraineeAssignments",
                column: "ApproveByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
