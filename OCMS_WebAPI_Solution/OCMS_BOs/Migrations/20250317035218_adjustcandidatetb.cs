using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class adjustcandidatetb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Candidate_Specialties_SpecialtyId",
                table: "Candidate");

            migrationBuilder.DropForeignKey(
                name: "FK_Candidate_Users_ImportByUserID",
                table: "Candidate");

            migrationBuilder.DropForeignKey(
                name: "FK_ExternalCertificates_Candidate_CandidateId",
                table: "ExternalCertificates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Candidate",
                table: "Candidate");

            migrationBuilder.RenameTable(
                name: "Candidate",
                newName: "Candidates");

            migrationBuilder.RenameIndex(
                name: "IX_Candidate_SpecialtyId",
                table: "Candidates",
                newName: "IX_Candidates_SpecialtyId");

            migrationBuilder.RenameIndex(
                name: "IX_Candidate_ImportByUserID",
                table: "Candidates",
                newName: "IX_Candidates_ImportByUserID");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Requests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImportRequestId",
                table: "Candidates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Candidates",
                table: "Candidates",
                column: "CandidateId");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 3, 52, 16, 645, DateTimeKind.Utc).AddTicks(8267), new DateTime(2025, 3, 17, 10, 52, 16, 645, DateTimeKind.Local).AddTicks(8263) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6885), "$2a$11$.7A86hY53MBXpO5xtfRiFekdRIVVH81DfcP3p.TcgDBQSh9vDQXRK", new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6885) });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Address", "CreatedAt", "DateOfBirth", "DepartmentId", "Email", "FullName", "Gender", "LastLogin", "PasswordHash", "PhoneNumber", "RoleId", "SpecialtyId", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { "AOC-1", "505 AOC Street", new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6925), new DateTime(1975, 11, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "aocmanager@gmail.com", "AOC Manager User", "Male", null, "$2a$11$PQS15gyKR12c49tEDtQM6e4pluu1PBXiGKHmGJ.wCXF8AXwbTil4O", "6677889900", 8, "SPEC-001", new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6925), "AOCManager" },
                    { "HM-1", "456 Headmaster Street", new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6891), new DateTime(1980, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "headmaster@gmail.com", "Head Master User", "Male", null, "$2a$11$PQS15gyKR12c49tEDtQM6e4pluu1PBXiGKHmGJ.wCXF8AXwbTil4O", "0987654321", 2, "SPEC-001", new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6891), "HeadMaster" },
                    { "HR-1", "101 HR Street", new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6897), new DateTime(1985, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "hrmanager@gmail.com", "HR Manager", "Male", null, "$2a$11$PQS15gyKR12c49tEDtQM6e4pluu1PBXiGKHmGJ.wCXF8AXwbTil4O", "2233445566", 4, "SPEC-001", new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6898), "HRManager" },
                    { "INST-1", "202 Instructor Avenue", new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6900), new DateTime(1990, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "instructor@gmail.com", "Instructor User", "Female", null, "$2a$11$PQS15gyKR12c49tEDtQM6e4pluu1PBXiGKHmGJ.wCXF8AXwbTil4O", "3344556677", 5, "SPEC-001", new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6901), "Instructor" },
                    { "REV-1", "303 Reviewer Blvd", new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6903), new DateTime(1993, 12, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "reviewer@gmail.com", "Reviewer User", "Male", null, "$2a$11$PQS15gyKR12c49tEDtQM6e4pluu1PBXiGKHmGJ.wCXF8AXwbTil4O", "4455667788", 6, "SPEC-001", new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6904), "Reviewer" },
                    { "TR-1", "404 Trainee Lane", new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6921), new DateTime(2002, 8, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "trainee@gmail.com", "Trainee User", "Female", null, "$2a$11$PQS15gyKR12c49tEDtQM6e4pluu1PBXiGKHmGJ.wCXF8AXwbTil4O", "5566778899", 7, "SPEC-001", new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6922), "Trainee" },
                    { "TS-1", "789 Training Staff Lane", new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6894), new DateTime(1992, 7, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "trainingstaff@gmail.com", "Training Staff User", "Female", null, "$2a$11$PQS15gyKR12c49tEDtQM6e4pluu1PBXiGKHmGJ.wCXF8AXwbTil4O", "1122334455", 3, "SPEC-001", new DateTime(2025, 3, 17, 3, 52, 17, 278, DateTimeKind.Utc).AddTicks(6894), "TrainingStaff" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_ImportRequestId",
                table: "Candidates",
                column: "ImportRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Candidates_Requests_ImportRequestId",
                table: "Candidates",
                column: "ImportRequestId",
                principalTable: "Requests",
                principalColumn: "RequestId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidates_Specialties_SpecialtyId",
                table: "Candidates",
                column: "SpecialtyId",
                principalTable: "Specialties",
                principalColumn: "SpecialtyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidates_Users_ImportByUserID",
                table: "Candidates",
                column: "ImportByUserID",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalCertificates_Candidates_CandidateId",
                table: "ExternalCertificates",
                column: "CandidateId",
                principalTable: "Candidates",
                principalColumn: "CandidateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Candidates_Requests_ImportRequestId",
                table: "Candidates");

            migrationBuilder.DropForeignKey(
                name: "FK_Candidates_Specialties_SpecialtyId",
                table: "Candidates");

            migrationBuilder.DropForeignKey(
                name: "FK_Candidates_Users_ImportByUserID",
                table: "Candidates");

            migrationBuilder.DropForeignKey(
                name: "FK_ExternalCertificates_Candidates_CandidateId",
                table: "ExternalCertificates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Candidates",
                table: "Candidates");

            migrationBuilder.DropIndex(
                name: "IX_Candidates_ImportRequestId",
                table: "Candidates");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ImportRequestId",
                table: "Candidates");

            migrationBuilder.RenameTable(
                name: "Candidates",
                newName: "Candidate");

            migrationBuilder.RenameIndex(
                name: "IX_Candidates_SpecialtyId",
                table: "Candidate",
                newName: "IX_Candidate_SpecialtyId");

            migrationBuilder.RenameIndex(
                name: "IX_Candidates_ImportByUserID",
                table: "Candidate",
                newName: "IX_Candidate_ImportByUserID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Candidate",
                table: "Candidate",
                column: "CandidateId");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 16, 10, 44, 8, 162, DateTimeKind.Utc).AddTicks(9436), new DateTime(2025, 3, 16, 17, 44, 8, 162, DateTimeKind.Local).AddTicks(9435) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 16, 10, 44, 8, 287, DateTimeKind.Utc).AddTicks(8053), "$2a$11$0YLDR4NFLfrCK5.hhNE9b.7ko5qal.t3MNB2n4Dz8ujjYxaaBrY3C", new DateTime(2025, 3, 16, 10, 44, 8, 287, DateTimeKind.Utc).AddTicks(8054) });

            migrationBuilder.AddForeignKey(
                name: "FK_Candidate_Specialties_SpecialtyId",
                table: "Candidate",
                column: "SpecialtyId",
                principalTable: "Specialties",
                principalColumn: "SpecialtyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidate_Users_ImportByUserID",
                table: "Candidate",
                column: "ImportByUserID",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalCertificates_Candidate_CandidateId",
                table: "ExternalCertificates",
                column: "CandidateId",
                principalTable: "Candidate",
                principalColumn: "CandidateId");
        }
    }
}
